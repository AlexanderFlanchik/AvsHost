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
    Stream? ContentStream = null)
{
    public SiteError ToSiteError() => 
        new SiteError 
        { 
            Id = SiteId,
            Error = ErrorMessage!, 
            Statuscode = StatusCode,
            SiteOwnerId = OwnerId,
        };

    public static HandleContentResult NotFound(string errorMessage, string? siteId = null, string? siteOwner = null)
    {
        return new HandleContentResult((int)HttpStatusCode.NotFound, errorMessage, siteId, null, siteOwner);
    }

    public static HandleContentResult ContentBlocked(string errorMessage, string siteId, string? siteOwner = null)
    {
        return new HandleContentResult((int)HttpStatusCode.BadRequest, errorMessage, siteId, null, siteOwner);
    }

    public static HandleContentResult Success(string siteId, IFileInfo fileInfo, string contentType)
    {
        return new HandleContentResult((int)HttpStatusCode.OK, null, siteId, fileInfo, contentType);
    }

    public static HandleContentResult Success(string siteId, Stream contentStream, string contentType)
    {
        return new HandleContentResult((int)HttpStatusCode.OK, null, siteId, null, contentType, ContentStream: contentStream);
    }
}