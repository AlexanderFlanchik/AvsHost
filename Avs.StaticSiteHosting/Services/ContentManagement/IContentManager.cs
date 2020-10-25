using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using System.Collections.Generic;
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
        Task<(string, string)> GetContentFileAsync(string contentItemId);

        /// <summary>
        /// Updates content stored in the site data storage and corresponding record in db.
        /// </summary>
        /// <param name="contentItemId">Content item ID</param>
        /// <param name="content">Content</param>
        /// <returns></returns>
        Task UpdateContentItem(string contentItemId, string content);

        Task<bool> DeleteContentByIdAsync(string contentItemId);
        bool DeleteNewUploadedFile(string fileName, string uploadSessionId);
    }
}