using System.Net;
using Avs.StaticSiteHosting.Shared.Contracts;
using Microsoft.Extensions.FileProviders;

namespace Avs.StaticSiteHosting.ContentHost.Contracts;

public record HandleContentResult(
    int StatusCode, 
    string? ErrorMessage = null, 
    string? SiteId = null, 
    IFileInfo? Content = null, 
    string? ContentType = null,
    string? OwnerId = null,
    Stream? ContentStream = null,
    TimeSpan? CacheDuration = null)
{
    public SiteError ToSiteError() => 
        new SiteError 
        { 
            Id = SiteId,
            Error = ErrorMessage!, 
            Statuscode = StatusCode,
            SiteOwnerId = OwnerId,
        };

    public static HandleContentResult NotFound(
        string errorMessage,
        string? siteId = null,
        string? siteOwnerId = null)
        => new HandleContentResult(
            (int)HttpStatusCode.NotFound,
            errorMessage, siteId,
            null,
            OwnerId: siteOwnerId);

    public static HandleContentResult ContentBlocked(
        string errorMessage,
        string siteId,
        string? siteOwnerId = null)
        => new HandleContentResult(
            (int)HttpStatusCode.BadRequest,
            errorMessage,
            siteId,
            null,
            OwnerId: siteOwnerId);

    public static HandleContentResult Success(
        string siteId,
        IFileInfo fileInfo,
        string contentType,
        TimeSpan? cacheDuration = null)
        => new HandleContentResult(
            (int)HttpStatusCode.OK,
            null,
            siteId,
            fileInfo,
            contentType,
            CacheDuration: cacheDuration);

    public static HandleContentResult Success(
        string siteId,
        Stream contentStream,
        string contentType,
        TimeSpan? cacheDuration = null)
        => new HandleContentResult(
            (int)HttpStatusCode.OK,
            null,
            siteId,
            null,
            contentType,
            ContentStream: contentStream,
            CacheDuration: cacheDuration);
}