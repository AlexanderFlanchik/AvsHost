using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;

namespace Avs.StaticSiteHosting.Web.Middlewares
{
    /// <summary>
    /// General middleware which serves static site content.
    /// </summary>
    public class StaticSiteMiddleware
    {
        private const string INVALID_SITE = "Invalid Site";
        private const string LOCAL_IP = "::1";
        
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly ISiteStatisticsService _siteStatisticsService;
        private readonly ICloudStorageProvider _cloudStorageProvider;
        private readonly IEventLogsService _eventLogsService;
        private readonly CloudStorageSettings _cloudStorageSettings;

        public StaticSiteMiddleware(RequestDelegate next, 
            ISiteService siteService, 
            IContentManager contentManager,
            ISiteStatisticsService siteStatisticsService,
            ICloudStorageProvider cloudStorageProvider,
            IEventLogsService eventLogsService,
            CloudStorageSettings cloudStorageSettings)
        {
             _siteService = siteService;
            _contentManager = contentManager;
            _siteStatisticsService = siteStatisticsService;
            _cloudStorageProvider = cloudStorageProvider;
            _eventLogsService = eventLogsService;
            _cloudStorageSettings = cloudStorageSettings;
        }

        public async Task Invoke(HttpContext context, IOptions<StaticSiteOptions> staticSiteOptions, ILogger<StaticSiteMiddleware> logger)
        {            
            var routeValues = context.GetRouteData().Values;
            if (await HandleDashboardContent(context))
            {
                return;
            }

            var siteName = (string)routeValues["sitename"];
            var sitePath = (string)routeValues["sitepath"];

            var siteInfo = await GetSiteAsync(siteName);
            var siteContentPath = Path.Combine(staticSiteOptions.Value.ContentPath, siteName);

            await CheckContentPathAndSiteStatus(siteContentPath, siteInfo);

            var (contentItem, fileName) = await GetContentItem(siteInfo, siteName, sitePath);
            if (contentItem is not null)
            {
                var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
                var fileInfo = fileProvider.GetFileInfo(fileName);
                
                if (!fileInfo.Exists)
                {
                    await ProcessCloudContent(context, siteInfo, contentItem, fileInfo);
                    return;
                }

                await CheckIfSiteIsViewed(fileInfo, siteInfo, context);

                context.Response.ContentType = contentItem.ContentType;

                await context.Response.SendFileAsync(fileInfo);

                logger.LogInformation($"Content sent: {fileName}");
            }
            else
            {
                await AddErrorToEventLog(siteInfo.Id, $"Resource with name '{fileName}' was not found.");
                
                throw new StaticSiteProcessingException(404, "Oops, no content", $"No content with name {fileName} found.");
            }
        }

        #region Private Methods

        private async Task<Site> GetSiteAsync(string siteName)
        {
            var siteInfo = await _siteService.GetSiteByNameAsync(siteName);
            if (siteInfo == null)
            {
                throw new StaticSiteProcessingException(404, "Oops, no such site!", $"No site with name '{siteName}' found.");
            }

            if (!siteInfo.IsActive)
            {
                await AddErrorToEventLog(siteInfo.Id, "The site has been turned off.");

                throw new StaticSiteProcessingException(503, "Access Denied", "This site is not available.");
            }

            return siteInfo;
        }

        private async ValueTask CheckContentPathAndSiteStatus(string siteContentPath, Site siteInfo)
        {
            if (!Directory.Exists(siteContentPath))
            {
                var errMessage = $"No content folder found for site named '{siteInfo.Name}'";
                await AddErrorToEventLog(siteInfo.Id, errMessage);

                throw new StaticSiteProcessingException(404, INVALID_SITE, errMessage);
            }

            var siteOwner = siteInfo.CreatedBy;
            if (siteOwner.Status != UserStatus.Active)
            {
                await AddErrorToEventLog(siteInfo.Id, "Site cannot be processed because of lock of its owner.");

                throw new StaticSiteProcessingException(400, INVALID_SITE, "This content cannot be provided because its owner has been blocked.");
            }
        }

        private async Task<(ContentItemModel item, string fileName)> GetContentItem(Site siteInfo, string siteName, string sitePath)
        {
            var contentItems = await _contentManager.GetUploadedContentAsync(siteInfo.Id);
            if (contentItems == null || !contentItems.Any())
            {
                await AddErrorToEventLog(siteInfo.Id, "Unable to find content for site.");

                throw new StaticSiteProcessingException(404, "Oops, no content", $"No content found for site named '{siteName}'");
            }

            var mappedSitePath = siteInfo.Mappings?.FirstOrDefault(m => m.Key == sitePath).Value;
            var fileName = mappedSitePath != null ? mappedSitePath : sitePath;
            var contentItem = contentItems.FirstOrDefault(ci => 
                ci.FileName == fileName || $"{ci.DestinationPath.Replace('\\', '/')}/{ci.FileName}" == fileName
             );

            return (contentItem, fileName);
        }

        private async Task ProcessCloudContent(HttpContext context, Site siteInfo, ContentItemModel contentItem, IFileInfo fileInfo)
        {
            if (!_cloudStorageSettings.Enabled)
            {
                await NoCloudContentError();
            }

            // file does not exist in the application local storage, check it in the cloud bucket
            using var cloudStream = await _cloudStorageProvider.GetCloudContent(siteInfo.CreatedBy.Name, siteInfo.Name, fileInfo.Name);
            if (cloudStream is null)
            {
                await NoCloudContentError();
            }

            context.Response.ContentType = contentItem.ContentType;
            await cloudStream.CopyToAsync(context.Response.Body);

            context.RequestServices.GetService<CloudStorageSyncWorker>()
                .Add(
                    new SyncContentTask
                    {
                        ContentName = fileInfo.Name,
                        FullFileName = fileInfo.PhysicalPath,
                        UserName = siteInfo.CreatedBy.Name,
                        SiteName = siteInfo.Name
                    });

            async Task NoCloudContentError()
            {
                var errorMessage = $"No content {fileInfo.Name} for site {siteInfo.Name}.";
                await AddErrorToEventLog(siteInfo.Id, errorMessage);

                throw new StaticSiteProcessingException(404, "Oops, no content", errorMessage);
            }
        }

        private async ValueTask CheckIfSiteIsViewed(IFileInfo fi, Site siteInfo, HttpContext context)
        {
            var landingPage = siteInfo.LandingPage;
            if (!string.IsNullOrEmpty(landingPage) && fi.Name == landingPage)
            {
                var visitor = context.Connection.RemoteIpAddress?.ToString() ?? LOCAL_IP;
                await _siteStatisticsService.MarkSiteAsViewed(siteInfo.Id, visitor);
            }
        }

        private async ValueTask<bool> HandleDashboardContent(HttpContext context)
        {
            var distPath = Path.Combine(new DirectoryInfo("ClientApp").FullName, "dist");
            var reqPath = context.Request.Path;
            
            var filePath = Path.Combine(distPath, reqPath);
            var fileProvider = new PhysicalFileProvider(distPath);

            var fi = fileProvider.GetFileInfo(filePath);
            if (fi.Exists)
            {
                context.Response.ContentType = _contentManager.GetContentType(fi.Name);
                await context.Response.SendFileAsync(fi);
                
                return true;
            }
            
            return false;
        }

        private Task AddErrorToEventLog(string siteId, string errorMessage)
                => _eventLogsService.InsertSiteEventAsync(siteId, INVALID_SITE, Models.SiteEventType.Error, errorMessage);

        #endregion
    }

    public static class StaticSiteMiddlewareExtentions
    {
        public static IEndpointConventionBuilder MapStaticSite(this IEndpointRouteBuilder endpoints, string pattern)
        {
            var applicationBuilder = endpoints.CreateApplicationBuilder()
                .UseMiddleware<StaticSiteMiddleware>()
                .Build();

            return endpoints.Map(pattern, applicationBuilder)
                    .WithDisplayName("Static site");
        }
    }
}