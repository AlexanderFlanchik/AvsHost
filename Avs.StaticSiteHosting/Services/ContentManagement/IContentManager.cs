using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public interface IContentManager
    {
        /// <summary>
        /// Uploads new files from upload with given id for the site specified.
        /// </summary>
        /// <param name="site">Site reference</param>
        /// <param name="uploadSessionId">Upload session id</param>
        /// <returns></returns>
        Task ProcessSiteContentAsync(Site site, string uploadSessionId);
        
        /// <summary>
        /// Returns a list of content files for the site specified.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns></returns>
        Task<IEnumerable<ContentItemModel>> GetUploadedContentAsync(string siteId);

        /// <summary>
        /// Deletes site content.
        /// </summary>
        /// <param name="site">Site reference</param>
        /// <returns></returns>
        Task DeleteSiteContentAsync(Site site);

        /// <summary>
        /// Returns a content resource reference related to Id specified, or null if content item not found
        /// </summary>
        /// <param name="contentItemId">Content item ID</param>
        /// <returns></returns>
        Task<(string, string, Stream)> GetContentFileAsync(string contentItemId);

        /// <summary>
        /// Updates content stored in the site data storage and corresponding record in db.
        /// </summary>
        /// <param name="contentItemId">Content item ID</param>
        /// <param name="content">Content</param>
        /// <returns></returns>
        Task<long> UpdateContentItem(string contentItemId, string content);

        /// <summary>
        /// Deletes content using content item ID.
        /// </summary>
        /// <param name="contentItemId"></param>
        /// <returns></returns>
        Task<bool> DeleteContentByIdAsync(string contentItemId);
        
        /// <summary>
        /// Deletes a new uploaded file.
        /// </summary>
        /// <param name="fileName">File name to delete</param>
        /// <param name="uploadSessionId">Upload session ID.</param>
        /// <returns>True if deletion succeded, otherwise false.</returns>
        bool DeleteNewUploadedFile(string fileName, string uploadSessionId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<StorageUsedInfo>> GetUsedStorageAmountByUser(string userId);

        /// <summary>
        /// Returns a content type if it can be extracted from a file name, "application/octet-stream" otherwise
        /// </summary>
        /// <param name="contentFileName">Content file name</param>
        /// <returns>A content type as string.</returns>
        string GetContentType(string contentFileName);

        /// <summary>
        /// Get a size of a file just uploaded.
        /// </summary>
        /// <param name="contentFileName">File name.</param>
        /// <param name="uploadSessionId">Upload session ID.</param>
        /// <returns>File size in bytes.</returns>
        long GetNewFileSize(string contentFileName, string uploadSessionId);
    }
}