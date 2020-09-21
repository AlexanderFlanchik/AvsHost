using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services.ContentManagement
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
    }
}