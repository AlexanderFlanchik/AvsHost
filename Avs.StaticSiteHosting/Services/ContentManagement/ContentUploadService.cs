using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

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
        /// <returns></returns>
        Task UploadContent(string uploadSessionId, string fileName, string destinationPath, Stream content);
        
        /// <summary>
        /// Upload string content to content or temporary storage.
        /// </summary>
        /// <param name="uploadSessionId">Upload session ID</param>
        /// <param name="fileName">Uploaded file name</param>
        /// <param name="destinationPath">Destination path</param>
        /// <param name="content">The content to be uploaded</param>
        /// <returns></returns>
        Task UploadContent(string uploadSessionId, string fileName, string destinationPath, string content);
    }

    public class ContentUploadService : IContentUploadService
    {
        private readonly StaticSiteOptions _staticSiteOptions;

        public ContentUploadService(IOptions<StaticSiteOptions> staticSiteOptions)
        {
            _staticSiteOptions = staticSiteOptions.Value;
        }

        public bool ValidateDestinationPath(string destinationPath)
        {
            if (string.IsNullOrEmpty(destinationPath))
            {
                return true;
            }
            
            if (destinationPath.StartsWith('\\') || destinationPath.StartsWith('/'))
            {
                return false;
            }

            return true;
        }

        public async Task UploadContent(string uploadSessionId, string fileName, string destinationPath, Stream content)
        {
            var newFilePath = GetNewFilePath(uploadSessionId, destinationPath, fileName);
            var fi = new FileInfo(newFilePath);
            using var fiStream = fi.OpenWrite();

            using (content)
            {
                await content.CopyToAsync(fiStream);
            }
        }

        public async Task UploadContent(string uploadSessionId, string fileName, string destinationPath, string content)
        {
            var newFilePath = GetNewFilePath(uploadSessionId, destinationPath, fileName);
            
            await File.WriteAllTextAsync(newFilePath, content);
        }

        private string GetNewFilePath(string uploadSessionId, string destinationPath, string fileName)
        {
            var tempContentPath = _staticSiteOptions.TempContentPath;
            destinationPath ??= string.Empty;
            destinationPath = destinationPath.Replace('/', '\\');

            string uploadFolderPath = Path.Combine(tempContentPath, uploadSessionId, destinationPath);
           
            Directory.CreateDirectory(uploadFolderPath);
            var newFilepath = Path.Combine(uploadFolderPath, fileName);
            
            return newFilepath;
        }
    }
}