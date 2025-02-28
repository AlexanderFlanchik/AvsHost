using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Shared.Configuration;
using System;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public interface IContentUploadService
    {
        public bool ValidateDestinationPath(string destinationPath);

        /// <summary>
        /// Uploads content to content item or temporary storage and disposes the input stream
        /// </summary>
        /// <param name="uploadSessionId">Upload session ID</param>
        /// <param name="fileName">Uploaded file name</param>
        /// <param name="destinationPath">Destination path</param>
        /// <param name="content">A content stream</param>
        /// <param name="cacheDuration">A cache duration if is needed</param>
        /// <returns></returns>
        Task UploadContent(string uploadSessionId, string fileName, string destinationPath, Stream content, TimeSpan? cacheDuration = null);
        
        /// <summary>
        /// Upload string content to content or temporary storage.
        /// </summary>
        /// <param name="uploadSessionId">Upload session ID</param>
        /// <param name="fileName">Uploaded file name</param>
        /// <param name="destinationPath">Destination path</param>
        /// <param name="content">The content to be uploaded</param>
        /// <param name="cacheDuration">A cache duration if is needed</param>
        /// <returns></returns>
        Task UploadContent(string uploadSessionId, string fileName, string destinationPath, string content, TimeSpan? cacheDuration = null);

        /// <summary>
        /// Gets all content upload entries for current session ID
        /// </summary>
        /// <param name="uploadSessionId">Upload session ID</param>
        /// <returns>A task which returns a list of content upload records when resolves.</returns>
        Task<IEnumerable<ContentUpload>> GetUploadInfoAsync(string uploadSessionId);

        /// <summary>
        /// Clears information about content uploads with current session ID
        /// </summary>
        /// <param name="uploadSessionId">Upload session ID</param>
        /// <returns></returns>
        Task ClearUploadInfo(string uploadSessionId);
    }

    public class ContentUploadService : IContentUploadService
    {
        private readonly StaticSiteOptions _staticSiteOptions;
        private readonly IMongoCollection<ContentUpload> _contentUploads;

        public ContentUploadService(IOptions<StaticSiteOptions> staticSiteOptions, MongoEntityRepository repository) 
        {
            _staticSiteOptions = staticSiteOptions.Value;
            _contentUploads = repository.GetEntityCollection<ContentUpload>(GeneralConstants.CONTENT_UPLOADS_COLLECTION);
        }

        public bool ValidateDestinationPath(string destinationPath)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                return true;
            }
            
            if (destinationPath.StartsWith(Path.DirectorySeparatorChar))
            {
                return false;
            }

            return true;
        }

        public async Task UploadContent(string uploadSessionId, string fileName, string destinationPath, Stream content, TimeSpan? cacheDuration = null)
        {
            var newFilePath = GetNewFilePath(uploadSessionId, destinationPath, fileName);
            var fi = new FileInfo(newFilePath);
            await using var fiStream = fi.OpenWrite();
            await using (content)
            {
                await content.CopyToAsync(fiStream);
            }

            await InsertContentUploadAsync(uploadSessionId, fileName, destinationPath, cacheDuration);
        }

        public async Task UploadContent(string uploadSessionId, string fileName, string destinationPath, string content, TimeSpan? cacheDuration = null)
        {
            var newFilePath = GetNewFilePath(uploadSessionId, destinationPath, fileName);
            
            await File.WriteAllTextAsync(newFilePath, content);
            await InsertContentUploadAsync(uploadSessionId, fileName, destinationPath, cacheDuration);
        }

        private async Task InsertContentUploadAsync(string uploadSessionId, string fileName, string destinationPath, TimeSpan? cacheDuration = null)
        {
            var entry = new ContentUpload
            {
                CacheDuration = cacheDuration,
                FileName = fileName,
                DestinationPath = destinationPath,
                UploadSessionId = uploadSessionId
            };

            await _contentUploads.InsertOneAsync(entry);
        }

        private string GetNewFilePath(string uploadSessionId, string destinationPath, string fileName)
        {
            var tempContentPath = _staticSiteOptions.TempContentPath;
            destinationPath ??= string.Empty;
            destinationPath = destinationPath.Replace('/', Path.DirectorySeparatorChar);

            string uploadFolderPath = Path.Combine(tempContentPath, uploadSessionId, destinationPath);
           
            Directory.CreateDirectory(uploadFolderPath);
            var newFilepath = Path.Combine(uploadFolderPath, fileName);
            
            return newFilepath;
        }

        public Task ClearUploadInfo(string uploadSessionId)
        {
            var filter = new FilterDefinitionBuilder<ContentUpload>().Where(u => u.UploadSessionId == uploadSessionId);
            return _contentUploads.FindOneAndDeleteAsync(filter);
        }

        public async Task<IEnumerable<ContentUpload>> GetUploadInfoAsync(string uploadSessionId)
        {
            var filter = new FilterDefinitionBuilder<ContentUpload>().Where(u => u.UploadSessionId == uploadSessionId);
            var query = await _contentUploads.FindAsync(filter);

            return await query.ToListAsync();
        }
    }
}