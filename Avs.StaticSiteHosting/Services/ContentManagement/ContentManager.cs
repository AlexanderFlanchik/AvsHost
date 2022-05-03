using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public class ContentManager : IContentManager
    {
        private readonly ILogger<ContentManager> _logger;
        private readonly IMongoCollection<ContentItem> contentItems;
        private readonly StaticSiteOptions options;
        private readonly ISiteService _siteService;
        private readonly ISettingsManager _settingsManager;

        public ContentManager(
            MongoEntityRepository entityRepository, 
            IOptions<StaticSiteOptions> staticSiteOptions, 
            ISiteService siteService,
            ISettingsManager settingsManager,
            ILogger<ContentManager> logger)
        {
            contentItems = entityRepository.GetEntityCollection<ContentItem>(GeneralConstants.CONTENT_ITEMS_COLLECTION);
            options = staticSiteOptions.Value;
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _logger = logger;
        }

        public async Task ProcessSiteContentAsync(Site site, string uploadSessionId)
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
                contentInfos = (await contentItems.FindAsync(s => s.Site.Id == site.Id).ConfigureAwait(false)).ToList();
            }

            void GetFilesFromFolder(string folder)
            {
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

            var cloudStorageEnabled = false;
            CloudStorageSettings cloudStorageSettings = null;
            var cloudStorageSettingsEntry = await _settingsManager.GetAsync(CloudStorageSettings.SettingsName);
            if (!string.IsNullOrEmpty(cloudStorageSettingsEntry?.Value))
            {
                cloudStorageSettings = JsonConvert.DeserializeObject<CloudStorageSettings>(cloudStorageSettingsEntry?.Value);
                cloudStorageEnabled = cloudStorageSettings.Enabled;
            }

            foreach (var file in fileList)
            {
                var fileInfo = new FileInfo(file);
                var pathSegments = new List<string>();

                var dir = fileInfo.Directory;
                while (dir != null && dir.FullName != uploadFolder)
                {
                    pathSegments.Add(dir.Name);
                    dir = dir.Parent;
                }
                             
                var destinationFolder = Path.Combine(siteFolder, string.Join('\\', pathSegments.ToArray()));
                Directory.CreateDirectory(destinationFolder);

                var destinationPath = Path.Combine(destinationFolder, fileInfo.Name);
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }

                fileInfo.CopyTo(destinationPath);
                if (cloudStorageEnabled && cloudStorageSettings is not null)
                {
                    var putObjectRequest = new PutObjectRequest()
                    {
                        BucketName = cloudStorageSettings.BucketName,
                        Key = $"{site.CreatedBy.Name}/{site.Name}/{fileInfo.Name}",
                        InputStream = fileInfo.OpenRead(),
                        AutoCloseStream = true
                    };

                    try
                    {
                        using var client = new AmazonS3Client(cloudStorageSettings.AccessKey, 
                                                              cloudStorageSettings.Secret, 
                                                              RegionEndpoint.GetBySystemName(cloudStorageSettings.Region));

                        var response = await client.PutObjectAsync(putObjectRequest);
                        if (response.HttpStatusCode == HttpStatusCode.OK)
                        {
                            _logger.LogInformation("The file '{0}' successfully saved to cloud storage", fileInfo.Name);
                        }
                        else
                        {
                            _logger.LogWarning("Unable to save '{0}' to cloud storage. The service responded with code '{1}'", 
                                fileInfo.Name, (int)response.HttpStatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unable to save '{0}' to cloud storage because of exception", fileInfo.Name);
                    }
                }    

                var contentItemFound = contentInfos.FirstOrDefault(ci => ci.FullName == destinationPath);
                if (contentItemFound == null)
                {
                    string contentType;
                    if (!ctpProvider.TryGetContentType(fileInfo.Name, out contentType))
                    {
                        contentType = "application/octet-stream";
                    }

                    // Insert a record about new content item uploaded
                    contentItemList.Add(new ContentItem()
                    {
                        Name = fileInfo.Name,
                        Site = site,
                        ContentType = contentType,
                        UploadedAt = DateTime.UtcNow,
                        Size = Math.Round((decimal)fileInfo.Length / 1024, 1),
                        FullName = destinationPath
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
                Directory.Delete(uploadFolder, true);
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Unable to store content.");
            }
        }        

        public async Task<IEnumerable<ContentItemModel>> GetUploadedContentAsync(string siteId)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Site.Id == siteId).ConfigureAwait(false);
            var contentList = await ciCursor.ToListAsync().ConfigureAwait(false);
            
            return contentList.Select(i => {
                var fi = new FileInfo(i.FullName);
                var siteFolder = new DirectoryInfo(Path.Combine(options.ContentPath, i.Site.Name));
                var destinationPath = fi.Directory.FullName.Replace(siteFolder.FullName, string.Empty);
                                    
                return new ContentItemModel
                    {
                        Id = i.Id,
                        FileName = i.Name,
                        ContentType = i.ContentType,
                        UploadedAt = i.UploadedAt,
                        Size = i.Size,
                        UpdateDate = i.UpdateDate,
                        DestinationPath = destinationPath.Length > 1 ? destinationPath.Substring(1) : destinationPath
                    };
            });
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

        public async Task<(string, string)> GetContentFileAsync(string contentItemId)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Id == contentItemId).ConfigureAwait(false);
            var contentItem = await ciCursor.FirstOrDefaultAsync().ConfigureAwait(false);

            if (contentItem == null)
            {
                return (null, null);
            }

            return (contentItem.ContentType, contentItem.FullName);
        }

        public async Task UpdateContentItem(string contentItemId, string content)
        {
            var ciCursor = await contentItems.FindAsync(i => i.Id == contentItemId).ConfigureAwait(false);
            var contentItem = await ciCursor.FirstOrDefaultAsync().ConfigureAwait(false);
            
            if (contentItem == null)
            {
                return;
            }


            var contentFile = new FileInfo(contentItem.FullName);
            if (!contentFile.Exists)
            {
                return;
            }

            await File.WriteAllTextAsync(contentFile.FullName, content).ConfigureAwait(false);
            await contentItems.UpdateOneAsync(new FilterDefinitionBuilder<ContentItem>().Where(i => i.Id == contentItem.Id),
                new UpdateDefinitionBuilder<ContentItem>()
                    .Set(c => c.UpdateDate, DateTime.UtcNow));
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
            var siteIdsFilter = new FilterDefinitionBuilder<ContentItem>().In(s => s.Site.Id, siteIds);
            var projection = Builders<ContentItem>.Projection.Expression(ci => 
                new StorageUsedInfo 
                    { 
                        SiteId = ci.Site.Id, 
                        SiteName = ci.Site.Name, 
                        Bytes = (long)(ci.Size * 1024)
                    }
                );
            
            var lst = await contentItems.Find(siteIdsFilter).Project(projection).ToListAsync();
            
            // TODO: move aggregation to the query above
            var result = lst
                            .GroupBy(g => new { g.SiteId, g.SiteName })
                            .Select(i => new StorageUsedInfo 
                                            { 
                                                SiteId = i.Key.SiteId, 
                                                SiteName = i.Key.SiteName, 
                                                Bytes = i.Sum(b => b.Bytes) 
                                            }
                            ).ToList();
            
            return result;
        }
    }
}