using System.Net;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Contracts;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares;

public class SiteContentMiddleware(
    ISiteContentHandler handler,
    ContentCacheService cacheService,
    ISiteEventPublisher siteEventPublisher,
    IErrorPageHandler errorPageHandler,
    ILogger<SiteContentMiddleware> logger) : IMiddleware
{
    private const string CONTENT_TYPE_DEFAULT = "application/octet-stream";
    private const string LOCAL_IP = "::1";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method != HttpMethods.Get)
        {
            await next(context);
            return;
        }

        var siteItem = (SiteInfoItem)context.Items[Constants.SiteInfo]!;
        var handleResult = await handler.ValidateAndProcessContent(siteItem.SiteContentInfo, siteItem.SiteContentInfo?.Name ?? string.Empty, siteItem.SitePath!);

        if (handleResult.StatusCode != (int)HttpStatusCode.OK)
        {
            logger.LogError(handleResult.ErrorMessage);
            context.Response.StatusCode = handleResult.StatusCode;
            if (!string.IsNullOrEmpty(handleResult.SiteId) && !string.IsNullOrEmpty(handleResult.OwnerId))
            {
                siteEventPublisher.PublishEvent(handleResult.ToSiteError());
            }

            await errorPageHandler.Handle(context, handleResult.ErrorMessage!);

            return;
        }

        var contentStream = handleResult.Content is not null ? handleResult.Content.CreateReadStream() : handleResult.ContentStream!;

        await WriteStreamAsync(contentStream, context.Response, handleResult.ContentType);
        if (handleResult.CacheDuration.HasValue)
        {
            cacheService.AddContentToCache(siteItem.ContentCacheKey!, handleResult.ContentType!, contentStream, handleResult.CacheDuration.Value); 
        }
        else
        {
            await contentStream.DisposeAsync();
        }

        if (!siteItem.SiteContentInfo!.IsSiteVisited(siteItem.SitePath!))
        {
            return;
        }

        var visitor = context.Connection.RemoteIpAddress?.ToString() ?? LOCAL_IP;
        siteEventPublisher.PublishEvent(new SiteVisited(siteItem.SiteContentInfo!.Id, visitor, siteItem.SiteContentInfo.User.Id));
    }
    
    private async Task WriteStreamAsync(Stream stream, HttpResponse response, string? contentType)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = contentType ?? CONTENT_TYPE_DEFAULT;
        await stream.CopyToAsync(response.Body);
        stream.Position = 0;

        await response.Body.FlushAsync();
    }
}