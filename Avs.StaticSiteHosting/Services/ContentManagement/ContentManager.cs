﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Configuration;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public class ContentManager : IContentManager
    {
        private readonly ILogger<ContentManager> _logger;
        private readonly IMongoCollection<ContentItem> contentItems;
        private readonly StaticSiteOptions options;
        private readonly ISiteService _siteService;
        private readonly CloudStorageSettings _cloudStorageSettings;
        private readonly ICloudStorageProvider _cloudStorageProvider;
        private readonly IContentUploadService _contentUploadService;

        public ContentManager(
            MongoEntityRepository entityRepository,
            IOptions<StaticSiteOptions> staticSiteOptions,
            ISiteService siteService,
            ILogger<ContentManager> logger,
            ICloudStorageProvider cloudStorageProvider,
            CloudStorageSettings cloudStorageSettings,
            IContentUploadService contentUploadService)
        {
            contentItems = entityRepository.GetEntityCollection<ContentItem>(GeneralConstants.CONTENT_ITEMS_COLLECTION);
            options = staticSiteOptions.Value;
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _logger = logger;
            _cloudStorageSettings = cloudStorageSettings ?? throw new ArgumentNullException(nameof(cloudStorageSettings));

            _cloudStorageProvider = cloudStorageProvider;
            _contentUploadService = contentUploadService;
        }

        public async Task<IEnumerable<ContentItemModel>> ProcessSiteContentAsync(Site site, string uploadSessionId)
        {
            var uploadFolder = Path.Combine(options.TempContentPath, uploadSessionId);
            var siteFolder = Path.Combine(options.ContentPath, site.Name);

            Directory.CreateDirectory(siteFolder);

            var contentItemList = new List<ContentItem>();
            var fileList = new List<string>();
            var contentInfos = new List<ContentItem>();
            var contentInfosToUpdate = new List<ContentItem>();

            if (!string.IsNullOrEmpty(site.Id))
            {
                contentInfos = (await contentItems.FindAsync(s => s.Site.Id == site.Id)).ToList();
            }

            void GetFilesFromFolder(string folder)
            {
                if (!Directory.Exists(folder))
                {
                    return;
                }

                var entries = Directory.EnumerateFileSystemEntries(folder);
                foreach (var entry in entries)
                {
                    bool entryIsDirectory = (File.GetAttributes(entry) & FileAttributes.Directory) == FileAttributes.Directory;
                    if (entryIsDirectory)
                    {
                        GetFilesFromFolder(entry);
                    }
                    else
                    {
                        fileList.Add(entry);
                    }
                }
            }

            GetFilesFromFolder(uploadFolder);
            FileExtensionContentTypeProvider ctpProvider = new FileExtensionContentTypeProvider();
            var uploadFolderInfo = new DirectoryInfo(uploadFolder);
            var uploadInfos = await _contentUploadService.GetUploadInfoAsync(uploadSessionId);

            foreach (var file in fileList)
            {
                var fileInfo = new FileInfo(file);
                var pathSegments = new List<string>();

                var dir = fileInfo.Directory;
                while (dir != null && dir.FullName != uploadFolderInfo.FullName)
                {
                    pathSegments.Add(dir.Name);
                    dir = dir.Parent;
                }

                pathSegments.Reverse();
                var segmentsPath = string.Join(Path.DirectorySeparatorChar, pathSegments.ToArray());
                var destinationFolder = Path.Combine(siteFolder, segmentsPath);
                Directory.CreateDirectory(destinationFolder);

                var destinationPath = Path.Combine(destinationFolder, fileInfo.Name);
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                fileInfo.CopyTo(destinationPath);
                if (_cloudStorageSettings.Enabled)
                {
                    using var fileStream = fileInfo.OpenRead();
                    await _cloudStorageProvider.UploadContentToCloud(
                            site.CreatedBy.Name, 
                            site.Name, 
                            Path.Combine(segmentsPath, fileInfo.Name),
                            fileStream
                    );
                }

                var contentItemFound = contentInfos.FirstOrDefault(ci => ci.FullName == destinationPath);
                if (contentItemFound == null)
                {
                    string contentType;
                    if (!ctpProvider.TryGetContentType(fileInfo.Name, out contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    var contentUploadInfo = uploadInfos.FirstOrDefault(
                        u =>
                        {
                            bool matched = u.FileName == fileInfo.Name;
                            if (u.DestinationPath is null)
                            {
                                matched = matched && !pathSegments.Any();
                            }
                            else
                            {
                                var dpSegments = u.DestinationPath.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries).Order();
                                matched = matched && dpSegments.SequenceEqual(pathSegments.Order());
                            }

                            return matched;
                        });
                    
                    // Insert a record about new content item uploaded
                    contentItemList.Add(new ContentItem()
                    {
                        Name = fileInfo.Name,
                        Site = site,
                        ContentType = contentType,
                        UploadedAt = DateTime.UtcNow,
                        Size = Math.Round((decimal)fileInfo.Length / 1024, 1),
                        FullName = destinationPath,
                        CacheDuration = contentUploadInfo?.CacheDuration
                    });
                }
                else
                {
                    contentInfosToUpdate.Add(contentItemFound);
                }
            }

            if (contentItemList.Any())
            {
                await contentItems.InsertManyAsync(contentItemList).ConfigureAwait(false);
            }

            // Upload date update
            if (contentInfosToUpdate.Any())
            {
                var cIds = contentInfosToUpdate.Select(ci => ci.Id).ToList();
                var filter = new FilterDefinitionBuilder<ContentItem>().In(i => i.Id, cIds);
                var update = new UpdateDefinitionBuilder<ContentItem>().Set(ci => ci.UpdateDate, DateTime.UtcNow);

                await contentItems.UpdateManyAsync(filter, update).ConfigureAwait(false);
            }

            try
            {
                if (Directory.Exists(uploadFolder))
                {
                    Directory.Delete(uploadFolder, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to store content.");
            }

            return contentItemList.Select(FromEntity);
        }

        public async Task<IEnumerable<ContentItemModel>> GetUploadedContentAsync(string siteId)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Site.Id == siteId);
            var contentList = await ciCursor.ToListAsync();

            return contentList.Select(FromEntity);
        }

        public async Task DeleteSiteContentAsync(Site site)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Site.Id == site.Id).ConfigureAwait(false);
            var siteItems = await ciCursor.ToListAsync().ConfigureAwait(false);
            var siteFolder = Path.Combine(options.ContentPath, site.Name);

            if (!siteItems.Any())
            {
                return;
            }

            foreach (var si in siteItems)
            {
                var siInfo = new FileInfo(si.FullName);
                siInfo.Delete();

                var siFolder = siInfo.Directory;
                if (!siFolder.GetFileSystemInfos().Any())
                {
                    siFolder.Delete();
                }
            }

            if (Directory.Exists(siteFolder) && !Directory.GetFileSystemEntries(siteFolder).Any())
            {
                Directory.Delete(siteFolder);
            }

            var idsToDelete = siteItems.Select(i => i.Id).ToArray();
            var deleteFilter = new FilterDefinitionBuilder<ContentItem>().In(i => i.Id, idsToDelete);

            await contentItems.DeleteManyAsync(deleteFilter).ConfigureAwait(false);
        }

        public async Task<(string, string, Stream)> GetContentFileAsync(string contentItemId)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Id == contentItemId).ConfigureAwait(false);
            var contentItem = await ciCursor.FirstOrDefaultAsync().ConfigureAwait(false);

            if (contentItem == null)
            {
                return (null, null, null);
            }

            Stream stream = null;
            var fileInfo = new FileInfo(contentItem.FullName);
            if (fileInfo.Exists)
            {
                stream = fileInfo.OpenRead();
            }
            else
            {
                if (_cloudStorageSettings.Enabled)
                {
                    var site = contentItem.Site;
                    stream = await _cloudStorageProvider.GetCloudContent(site.CreatedBy.Name, site.Name, fileInfo.Name);
                }
            }

            return (fileInfo.Name, contentItem.ContentType, stream);
        }

        public async Task<long> UpdateContentItem(string contentItemId, string content, TimeSpan? cacheDuration)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Id == contentItemId).ConfigureAwait(false);
            var contentItem = await ciCursor.FirstOrDefaultAsync().ConfigureAwait(false);

            if (contentItem == null)
            {
                return 0;
            }


            var contentFile = new FileInfo(contentItem.FullName);
            if (!contentFile.Exists)
            {
                return 0;
            }
            
            var site = contentItem.Site;
            ReplaceResourcePlaceholders(ref content, site.Name);

            await File.WriteAllTextAsync(contentFile.FullName, content).ConfigureAwait(false);
            
            var fileInfo = new FileInfo(contentFile.FullName);
            if (_cloudStorageSettings.Enabled)
            {
                using var fileStream = fileInfo.OpenRead();
                await _cloudStorageProvider.UploadContentToCloud(site.CreatedBy.Name, site.Name, fileInfo.Name, fileStream);
            }

            await contentItems.UpdateOneAsync(
                    new FilterDefinitionBuilder<ContentItem>().Where(i => i.Id == contentItem.Id),
                    new UpdateDefinitionBuilder<ContentItem>().Set(c => c.UpdateDate, DateTime.UtcNow)
                                                              .Set(c => c.Size, Math.Round((decimal) fileInfo.Length / 1024, 2))
                                                              .Set(c => c.CacheDuration, cacheDuration));

            return contentFile.Length;
        }

        public async Task<bool> DeleteContentByIdAsync(string contentItemId)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Id == contentItemId).ConfigureAwait(false);
            var contentItem = await ciCursor.FirstOrDefaultAsync().ConfigureAwait(false);
            if (contentItem == null)
            {
                return false;
            }

            try
            {
                var fi = new FileInfo(contentItem.FullName);
                fi.Delete();
                if (_cloudStorageSettings.Enabled)
                {
                    var site = contentItem.Site;
                    await _cloudStorageProvider.DeleteContentFromCloud(site.CreatedBy.Name, site.Name, fi.Name);
                }

                await contentItems.DeleteOneAsync(new FilterDefinitionBuilder<ContentItem>().Where(i => i.Id == contentItemId)).ConfigureAwait(false);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool DeleteNewUploadedFile(string fileName, string uploadSessionId)
        {
            var filePath = Path.Combine(options.TempContentPath, uploadSessionId, fileName);
            var fileInfo = new FileInfo(filePath);

            try
            {
                fileInfo.Delete();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<StorageUsedInfo>> GetUsedStorageAmountByUser(string userId)
        {
            var siteIds = await _siteService.GetSiteIdsByOwner(userId);
            var siteIdsFilter = new FilterDefinitionBuilder<ContentItem>().Where(s => siteIds.Contains(s.Site.Id));
            var projection = Builders<ContentItem>.Projection.Expression(ci =>
                new StorageUsedInfo
                {
                    SiteId = ci.Site.Id,
                    SiteName = ci.Site.Name,
                    Size = ci.Size
                }
            );

            var query = contentItems.Aggregate()
                .Match(siteIdsFilter)
                .Project(projection)
                .Group(
                    s => new { s.SiteId, s.SiteName }, 
                    g => new StorageUsedInfo
                    {
                        SiteId = g.Key.SiteId,
                        SiteName = g.Key.SiteName,
                        Size = g.Sum(b => b.Size)
                    });
               
            var result = await query.ToListAsync();

            return result;
        }

        public string GetContentType(string contentFileName)
        {
            const string APPLICATION_OCTET_STREAM = "application/octet-stream";

            var ctpProvider = new FileExtensionContentTypeProvider();

            string contentType;
            if (!ctpProvider.TryGetContentType(contentFileName, out contentType))
            {
                contentType = APPLICATION_OCTET_STREAM;
            }

            return contentType;
        }

        public long GetNewFileSize(string contentFileName, string uploadSessionId)
        {
            var uploadFolder = Path.Combine(options.TempContentPath, uploadSessionId);
            var fi = new FileInfo(Path.Combine(uploadFolder, contentFileName));
            if (!fi.Exists)
            {
                return 0;
            }

            return fi.Length;
        }

        public IEnumerable<string> GetNewUploadedFiles(string uploadSessionId, string contentFileExtension)
        {
            var uploadFolder = Path.Combine(options.TempContentPath, uploadSessionId);
            var filesList = new List<string>();

            GetFiles(uploadFolder, null);

            void GetFiles(string directory, string parent)
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (!directoryInfo.Exists)
                {
                    return;
                }

                var parentPathSegments = new List<string>();
                if (parent is not null)
                {
                    var parentDirectory = new DirectoryInfo(parent);
                    while (parentDirectory is not null && parentDirectory.FullName != uploadFolder)
                    {
                        parentPathSegments.Add(parentDirectory.Name);
                        parentDirectory = parentDirectory.Parent;
                    }
                }

                if (directory != uploadFolder)
                {
                    parentPathSegments.Add(directoryInfo.Name);
                }

                var parentPath = string.Join('/', parentPathSegments.ToArray());
                var files = directoryInfo.GetFiles().Where(f => f.Extension == $".{contentFileExtension}")
                    .Select(f => string.IsNullOrEmpty(parentPath) ? f.Name : $"{parentPath}/{f.Name}").ToList();
                
                filesList.AddRange(files);
                               
                var directories = new DirectoryInfo(directory).GetDirectories();
                foreach (var d in directories)
                {
                    GetFiles(d.FullName, directoryInfo.FullName);
                }
            }

            return filesList;
        }

        private void ReplaceResourcePlaceholders(ref string content, string siteName)
        {          
            var matches = Regex.Matches(content, @"""/(%NEW_RESOURCE%|%EXIST_RESOURCE%)\/\S*\?\S*""");
            foreach (Match match in matches)
            {
                var foundLink = match.Value;
                var newLink = Regex.Replace(foundLink, "(%NEW_RESOURCE%|%EXIST_RESOURCE%)", HttpUtility.UrlEncode(siteName));
                newLink = Regex.Replace(newLink, @"\?\S*", "\"");
                content = content.Replace(foundLink, newLink);
            }
        }

        private ContentItemModel FromEntity(ContentItem entity)
        {
            var fi = new FileInfo(entity.FullName);
            var siteFolder = new DirectoryInfo(Path.Combine(options.ContentPath, entity.Site.Name));
            var destinationPath = fi.Directory.FullName.Replace(siteFolder.FullName, string.Empty);

            return new ContentItemModel
            {
                Id = entity.Id,
                FileName = entity.Name,
                ContentType = entity.ContentType,
                UploadedAt = entity.UploadedAt,
                Size = entity.Size,
                UpdateDate = entity.UpdateDate,
                DestinationPath = destinationPath.Length > 1 ? destinationPath.Substring(1) : destinationPath,
                CacheDuration = entity.CacheDuration
            };
        }
    }
}