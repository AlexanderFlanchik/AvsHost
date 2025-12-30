using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares;

public class SiteMiddleware(ContentCacheService cacheService, ISiteContentProvider siteContentProvider) : IMiddleware
{
    private const string SITE_NAME = "sitename";
    private const string SITE_PATH = "sitepath";
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        (string siteName, string sitePath) = GetRouteParameters(context);
        string contentCacheKey = ContentCacheHelper.GetContentCacheKey(siteName, sitePath);
        
        if (context.Request.Method == HttpMethods.Get &&
            await HandleCachedContentAsync(context.Response, contentCacheKey))
        {
            return;
        }
        
        SiteContentInfo? siteInfo = await siteContentProvider.GetSiteContentByName(siteName);

        context.Items[Constants.SiteInfo] = new SiteInfoItem()
        {
            SiteContentInfo = siteInfo,
            SitePath = sitePath,
            ContentCacheKey = contentCacheKey
        };
        
        await next(context);
    }
    
    private (string siteName, string sitePath) GetRouteParameters(HttpContext context)
    {
        var routeValues = context.GetRouteData().Values;
        string siteName = string.Empty, sitePath = string.Empty;

        if (routeValues.TryGetValue(SITE_NAME, out var sn) && sn is not null)
        {
            siteName = sn.ToString()!;
        }

        if (routeValues.TryGetValue(SITE_PATH, out var sp) && sp is not null)
        {
            sitePath = sp.ToString()!;
        }

        return (siteName, sitePath);
    }
    
    private async Task<bool> HandleCachedContentAsync(HttpResponse response, string contentCacheKey)
    {
        var cachedContent = await cacheService.GetContentCacheAsync(contentCacheKey);
        if (cachedContent is null)
        {
            return false;
        }
        
        using var ms = new MemoryStream(cachedContent.Content);
        await StreamWriterHelper.WriteStreamAsync(ms, response, cachedContent.ContentType);
        
        return true;
    }
}