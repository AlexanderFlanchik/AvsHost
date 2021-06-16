using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public class ContentManager : IContentManager
    {
        private readonly IMongoCollection<ContentItem> contentItems;
        private readonly StaticSiteOptions options;
        private readonly ISiteService _siteService;
        public ContentManager(MongoEntityRepository entityRepository, IOptions<StaticSiteOptions> staticSiteOptions, ISiteService siteService)
        {
            contentItems = entityRepository.GetEntityCollection<ContentItem>(GeneralConstants.CONTENT_ITEMS_COLLECTION);
            options = staticSiteOptions.Value;
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
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
                Console.WriteLine(ex.Message);
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