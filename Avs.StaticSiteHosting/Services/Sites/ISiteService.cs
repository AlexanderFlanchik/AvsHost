using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using Avs.StaticSiteHosting.Models.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
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
        
        /// <summary>
        /// Checks if a site with the name given already exists.
        /// </summary>
        /// <param name="siteName">Site name to validate</param>
        /// <param name="siteId">Site id if the validation is performed for existing site.</param>
        /// <returns></returns>
        Task<bool> CheckSiteNameUsedAsync(string siteName, string siteId);

        Task<Site> CreateSiteAsync(Site newSite);

        Task<Site> GetSiteByIdAsync(string siteId);

        Task UpdateSiteAsync(Site siteToUpdate);
    }
}