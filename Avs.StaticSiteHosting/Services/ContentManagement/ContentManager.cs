using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services.ContentManagement
{
    public class ContentManager : IContentManager
    {
        private readonly IMongoCollection<ContentItem> contentItems;
        private readonly StaticSiteOptions options;

        public ContentManager(MongoEntityRepository entityRepository,  IOptions<StaticSiteOptions> staticSiteOptions)
        {
            contentItems = entityRepository.GetEntityCollection<ContentItem>(GeneralConstants.CONTENT_ITEMS_COLLECTION);
            options = staticSiteOptions.Value;
        }

        public async Task CreateSiteContentAsync(Site site, string uploadSessionId)
        {
            var uploadFolder = Path.Combine(options.TempContentPath, uploadSessionId);
            var uploadFolderInfo = new DirectoryInfo(uploadFolder);
            var siteFolder = Path.Combine(options.ContentPath, site.Name);
            
            Directory.CreateDirectory(siteFolder);

            var contentEntries = Directory.EnumerateFileSystemEntries(uploadFolder);
            var contentItemList = new List<ContentItem>();
            var fileList = new List<string>();

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

                var destinationFolder = Path.Combine(siteFolder, string.Join('\\', pathSegments.ToArray()));
                Directory.CreateDirectory(destinationFolder);

                var destinationPath = Path.Combine(destinationFolder, fileInfo.Name);
                fileInfo.CopyTo(destinationPath);

                contentItemList.Add(new ContentItem()
                    {
                        Name = fileInfo.Name,
                        Site = site,
                        ContentType = "application/octet-stream",
                        UploadedAt = DateTime.UtcNow,
                        FullName = destinationPath
                    });

                fileInfo.Delete();
                if (!fileInfo.Directory.GetFileSystemInfos().Any())
                {
                    fileInfo.Directory.Delete();
                }                
            }

            await contentItems.InsertManyAsync(contentItemList);

            Directory.Delete(uploadFolder);
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
                        UploadedAt = i.UploadedAt,
                        DestinationPath = destinationPath.Length > 1 ? destinationPath.Substring(1) : destinationPath
                    };
            });
        }
    }
}