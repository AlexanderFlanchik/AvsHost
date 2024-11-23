using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Net;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Configuration;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares
{
    public class SiteContentMiddleware(
        ISiteContentProvider siteContentProvider, 
        IOptions<StaticSiteOptions> staticSiteOptions,
        ISiteEventPublisher siteEventPublisher,
        CloudStorageSettings cloudStorageSettings,
        ICloudStorageProvider cloudStorageProvider,
        CloudStorageWorker storageWorker,
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
            HandleContentResult handleResult = await ValidateAndProcessContent(siteInfo, siteName, sitePath);

            if (handleResult.StatusCode == (int)HttpStatusCode.OK)
            {
                context.Response.ContentType = handleResult.ContentType ?? CONTENT_TYPE_DEFAULT;

                if (handleResult.Content is not null)
                {
                    await context.Response.SendFileAsync(handleResult.Content);
                }
                else
                {
                    await handleResult.ContentStream!.CopyToAsync(context.Response.Body);
                }

                if (siteInfo!.IsSiteVisited(sitePath))
                {
                    var visitor = context.Connection.RemoteIpAddress?.ToString() ?? LOCAL_IP;
                    siteEventPublisher.PublishEvent(new SiteVisited(siteInfo!.Id, visitor, siteInfo.User.Id));
                }
            }
            else
            {
                logger.LogError(handleResult.ErrorMessage);

                context.Response.StatusCode = handleResult.StatusCode;
                if (!string.IsNullOrEmpty(handleResult.SiteId))
                {
                    siteEventPublisher.PublishEvent(handleResult.ToSiteError());
                }
            }
        }

        private async ValueTask<HandleContentResult> ValidateAndProcessContent(SiteContentInfo? siteInfo, string siteName, string sitePath)
        {
            if (siteInfo is null)
            {
                return new HandleContentResult((int)HttpStatusCode.NotFound);
            }

            string siteOwner = siteInfo.User.UserName;
            if (!siteInfo.User.IsActive)
            {
                return HandleContentResult.ContentBlocked("Cannot provide content. Site owner is blocked.", siteInfo.Id, siteOwner);
            }

            var (contentItem, fileName) = siteInfo.GetContentItem(sitePath);
            if (contentItem is null)
            {
                return HandleContentResult.NotFound(
                    string.Format("No content with path '{0}' found.", sitePath), 
                    siteInfo.Id, 
                    siteOwner);
            }

            var siteContentPath = Path.Combine(staticSiteOptions.Value.ContentPath, siteName);
            var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
            var fileInfo = fileProvider.GetFileInfo(fileName);
            if (!fileInfo.Exists)
            {
                if (!cloudStorageSettings.Enabled)
                {
                    return HandleContentResult.NotFound(
                        string.Format("No physical file found for path '{0}'.", sitePath),
                        siteInfo.Id, 
                        siteOwner);
                }

                using var cloudStream = await cloudStorageProvider.GetCloudContent(siteInfo.User.UserName, siteInfo.Name, fileInfo.Name);
                if (cloudStream is null)
                {
                    return HandleContentResult.NotFound(
                        string.Format("No content file found: '{0}'.", fileInfo.Name),
                        siteInfo.Id, 
                        siteOwner);
                }

                storageWorker.Add(new SyncContentTask(fileInfo.Name, fileInfo.PhysicalPath!, siteInfo.User.UserName, siteInfo.Name));

                return HandleContentResult.Success(siteInfo.Id, cloudStream, contentItem.ContentType);
            }

            return HandleContentResult.Success(siteInfo.Id, fileInfo, contentItem.ContentType);
        }

        private record HandleContentResult(
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
    }
}