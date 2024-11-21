using Avs.StaticSiteHosting.Web.DTOs;
using System.Threading.Tasks;
using System;

namespace Avs.StaticSiteHosting.Web.Services.Sites
{
    public interface ISiteManagementService
    {
        /// <summary>
        /// Creates a site and processes its content.
        /// </summary>
        /// <param name="siteDetails">Site details DTO</param>
        /// <param name="userId">Site owner ID</param>
        /// <returns>A combined result of response and exception if occurs</returns>
        Task<(CreateSiteResponseModel, Exception)> CreateSiteAndProcessContentAsync(SiteDetailsModel siteDetails, string userId);

        /// <summary>
        /// Updates a site and processes new uploaded content
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="siteDetails">Site details DTO</param>
        /// <returns></returns>
        Task<(UpdateSiteResponseModel, Exception)> UpdateSiteAndProcessContent(string siteId, string userId, SiteDetailsModel siteDetails);
    }
}
