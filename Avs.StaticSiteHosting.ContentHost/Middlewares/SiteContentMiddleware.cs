using System.Net;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Contracts;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares;

public class SiteContentMiddleware(
    ISiteContentHandler handler,
    ISiteContentProvider siteContentProvider,
    ISiteEventPublisher siteEventPublisher,
    ILogger<SiteContentMiddleware> logger) : IMiddleware
{
    private const string CONTENT_TYPE_DEFAULT = "application/octet-stream";
    private const string LOCAL_IP = "::1";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var routeValues = context.GetRouteData().Values;
        string siteName = string.Empty, sitePath = string.Empty;

        if (routeValues.TryGetValue("sitename", out var sn) && sn is not null)
        {
            siteName = sn.ToString()!;
        }

        if (routeValues.TryGetValue("sitepath", out var sp) && sp is not null)
        {
            sitePath = sp.ToString()!;
        }

        SiteContentInfo? siteInfo = await siteContentProvider.GetSiteContentByName(siteName);
        HandleContentResult handleResult = await handler.ValidateAndProcessContent(siteInfo, siteName, sitePath);

        if (handleResult.StatusCode != (int)HttpStatusCode.OK)
        {
            logger.LogError(handleResult.ErrorMessage);

            context.Response.StatusCode = handleResult.StatusCode;
            if (!string.IsNullOrEmpty(handleResult.SiteId))
            {
                siteEventPublisher.PublishEvent(handleResult.ToSiteError());
            }

            return;
        }
        
        context.Response.ContentType = handleResult.ContentType ?? CONTENT_TYPE_DEFAULT;

        if (handleResult.Content is not null)
        {
            await context.Response.SendFileAsync(handleResult.Content);
        }
        else
        {
            await handleResult.ContentStream!.CopyToAsync(context.Response.Body);
        }

        if (!siteInfo!.IsSiteVisited((sitePath)))
        {
            return;
        }

        var visitor = context.Connection.RemoteIpAddress?.ToString() ?? LOCAL_IP;
        siteEventPublisher.PublishEvent(new SiteVisited(siteInfo!.Id, visitor, siteInfo.User.Id));
    }
}