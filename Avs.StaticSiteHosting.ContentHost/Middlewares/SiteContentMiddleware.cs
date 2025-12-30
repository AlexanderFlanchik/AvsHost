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
            await HandleContentErrorAsync(handleResult, context);

            return;
        }

        var contentStream = handleResult.Content is not null ? handleResult.Content.CreateReadStream() : handleResult.ContentStream!;

        await StreamWriterHelper.WriteStreamAsync(contentStream, context.Response, handleResult.ContentType);
        await HandleContentCachingAsync(siteItem, handleResult, contentStream);
        CheckSiteVisited(siteItem, context);
    }

    private async Task HandleContentErrorAsync(HandleContentResult handleResult, HttpContext context)
    {
        logger.LogError("Request failed: {errorMessage}", handleResult.ErrorMessage);
        context.Response.StatusCode = handleResult.StatusCode;
            
        if (!string.IsNullOrEmpty(handleResult.SiteId) && !string.IsNullOrEmpty(handleResult.OwnerId))
        {
            siteEventPublisher.PublishEvent(handleResult.ToSiteError());
        }

        await errorPageHandler.Handle(context, handleResult.ErrorMessage!);
    }
    
    private async Task HandleContentCachingAsync(SiteInfoItem siteItem, HandleContentResult handleResult,
        Stream contentStream)
    {
        if (handleResult.CacheDuration.HasValue)
        {
            cacheService.AddContentToCache(
                siteItem.ContentCacheKey!,
                handleResult.ContentType!,
                contentStream,
                handleResult.CacheDuration.Value); 
        }
        else
        {
            await contentStream.DisposeAsync();
        }
    }
    
    private void CheckSiteVisited(SiteInfoItem siteItem, HttpContext context)
    {
        if (!siteItem.SiteContentInfo!.IsSiteVisited(siteItem.SitePath!))
        {
            return;
        }

        var visitor = context.Connection.RemoteIpAddress?.ToString() ?? LOCAL_IP;
        siteEventPublisher.PublishEvent(new SiteVisited(siteItem.SiteContentInfo!.Id, visitor, siteItem.SiteContentInfo.User.Id));
    }
}