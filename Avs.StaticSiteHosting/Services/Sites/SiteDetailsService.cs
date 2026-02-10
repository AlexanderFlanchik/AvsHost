using System;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;

namespace Avs.StaticSiteHosting.Web.Services.Sites;

public class SiteDetailsService(ISiteService siteService, IContentManager contentManager) : ISiteDetailsService
{
    public async Task<(Exception, SiteDetailsResponse)> GetSiteDetailsAsync(string siteId, string currentUserId)
    {
        var site = await siteService.GetSiteByIdAsync(siteId);
        if (site is null)
        {
            return (new NotFoundException(), null);
        }

        if (currentUserId != site.CreatedBy.Id)
        {
            return (new UnauthorizedException(), null);
        }
        
        var uploadedFiles = await contentManager.GetUploadedContentAsync(siteId);
        var siteDetailsResponse = new SiteDetailsResponse()
        {
            SiteName = site.Name,
            Description = site.Description,
            IsActive = site.IsActive,
            ResourceMappings = site.Mappings,
            LandingPage = site.LandingPage,
            Uploaded = uploadedFiles.ToList(),
            TagIds = site.TagIds?.Select(x => x.Id).ToArray(),
            DatabaseName = site.DatabaseName,
            CustomRouteHandlers = site.CustomRouteHandlers.Select(handler => new CustomRouteHandlerModel
            (
                handler.Id,
                site.Id,
                handler.Name,
                handler.Method,
                handler.Path,
                handler.Body
            )).ToList()
        };

        return (null, siteDetailsResponse);
    }

    public Task<bool> CheckSiteNameUsedAsync(string siteName, string siteId)
    {
        return siteService.CheckSiteNameUsedAsync(siteId, siteName);
    }
}