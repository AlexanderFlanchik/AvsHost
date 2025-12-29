using System;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;

namespace Avs.StaticSiteHosting.Web.Services.Sites;

public interface ISiteDetailsService
{
    /// <summary>
    /// Gets site details related to site ID specified
    /// </summary>
    /// <param name="siteId">Site ID</param>
    /// <param name="currentUserId">Current user ID from auth ticket</param>
    /// <returns>A task which returns found site details if found or error otherwise</returns>
    Task<(Exception, SiteDetailsResponse)> GetSiteDetailsAsync(string siteId, string currentUserId);

    /// <summary>
    /// Checks if the given site name is used
    /// </summary>
    /// <param name="siteName">Site name to check</param>
    /// <param name="siteId">Site ID</param>
    /// <returns>A task which returns true/false when resolved</returns>
    Task<bool> CheckSiteNameUsedAsync(string siteName, string siteId);
}