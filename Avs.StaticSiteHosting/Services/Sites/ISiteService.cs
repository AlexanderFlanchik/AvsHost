using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Models.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services
{
    public interface ISiteService
    {
        /// <summary>
        /// Returns all sites that match the query.
        /// </summary>
        /// <param name="query">Query parameters</param>
        /// <returns></returns>
        Task<IEnumerable<Site>> GetSitesAsync(SitesQuery query);
        
        /// <summary>
        /// Calculates amount of sites created by user.
        /// </summary>
        /// <param name="ownerId">Owner ID</param>
        /// <returns></returns>
        Task<int> GetSitesAmountAsync(string ownerId = null);
        Task<int> GetActiveSitesAmountAsync(string ownerId = null);
        /// <summary>
        /// Checks if a site with the name given already exists.
        /// </summary>
        /// <param name="siteName">Site name to validate</param>
        /// <param name="siteId">Site id if the validation is performed for existing site.</param>
        /// <returns></returns>
        Task<bool> CheckSiteNameUsedAsync(string siteName, string siteId);

        /// <summary>
        /// Creates a site.
        /// </summary>
        /// <param name="newSite">New site (with empty ID)</param>
        /// <returns></returns>
        Task<Site> CreateSiteAsync(Site newSite);

        /// <summary>
        /// Returns site details by site ID provided.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns></returns>
        Task<Site> GetSiteByIdAsync(string siteId);

        /// <summary>
        /// Returns site details by site name provided.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns></returns>
        Task<Site> GetSiteByNameAsync(string siteName);

        /// <summary>
        /// Search sites by site name specified.
        /// </summary>
        /// <param name="name">Site name</param>
        /// <param name="ownerId">Owner ID (optional).</param>
        /// <returns></returns>
        Task<SitesSearchResponse> SearchSitesByName(string name, string ownerId = null);

        /// <summary>
        /// Updates the existing site.
        /// </summary>
        /// <param name="siteToUpdate">Site reference to update.</param>
        /// <returns></returns>
        Task UpdateSiteAsync(Site siteToUpdate);

        /// <summary>
        /// Toggles and return site status.
        /// </summary>
        /// <param name="Site">Site refrence.</param>
        /// <returns></returns>
        Task<bool> ToggleSiteStatusAsync(Site site);

        /// <summary>
        /// Deletes the site.
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task DeleteSiteAsync(string siteId);

        Task UpdateSitesStatusAsync(string ownerId, UserStatus status);
    }
}