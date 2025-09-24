using System.Net;
using Avs.Messaging.Contracts;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Contracts;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares;

public class SiteContentMiddleware(
    ISiteContentHandler handler,
    ContentCacheService cacheService,
    ISiteContentProvider siteContentProvider,
    ISiteEventPublisher siteEventPublisher,
    IMessagePublisher messagePublisher,
    IErrorPageHandler errorPageHandler,
    ILogger<SiteContentMiddleware> logger) : IMiddleware
{
    private const string CONTENT_TYPE_DEFAULT = "application/octet-stream";
    private const string LOCAL_IP = "::1";
    private const string SITE_NAME = "sitename";
    private const string SITE_PATH = "sitepath";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Method != HttpMethods.Get)
        {
            await next(context);
            return;
        }

        var (siteName, sitePath) = GetRouteParameters(context);
        string contentCacheKey = ContentCacheHelper.GetContentCacheKey(siteName, sitePath);
        if (await HandleCachedContentAsync(context.Response, contentCacheKey))
        {
            return;
        }

        SiteContentInfo? siteInfo = await siteContentProvider.GetSiteContentByName(siteName);
        HandleContentResult handleResult = await handler.ValidateAndProcessContent(siteInfo, siteName, sitePath);

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

        Stream contentStream = handleResult.Content is not null ? handleResult.Content.CreateReadStream() : handleResult.ContentStream!;

        await WriteStreamAsync(contentStream, context.Response, handleResult.ContentType);
        if (handleResult.CacheDuration.HasValue)
        {
            cacheService.AddContentToCache(
                contentCacheKey,
                handleResult.ContentType!,
                contentStream,
                handleResult.CacheDuration.Value); // the stream will be disposed later
        }
        else
        {
            contentStream.Dispose();
        }

        if (!siteInfo!.IsSiteVisited(sitePath))
        {
            return;
        }

        var visitor = context.Connection.RemoteIpAddress?.ToString() ?? LOCAL_IP;
        siteEventPublisher.PublishEvent(new SiteVisited(siteInfo!.Id, visitor, siteInfo.User.Id));
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
        await WriteStreamAsync(ms, response, cachedContent.ContentType);

        return true;
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