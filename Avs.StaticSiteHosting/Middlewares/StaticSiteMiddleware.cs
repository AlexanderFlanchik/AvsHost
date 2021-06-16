using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.StaticFiles;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;

namespace Avs.StaticSiteHosting.Web.Middlewares
{
    /// <summary>
    /// General middleware which serves static site content.
    /// </summary>
    public class StaticSiteMiddleware
    {
        private const string INVALID_SITE = "Invalid Site";
        private readonly IOptions<StaticSiteOptions> _staticSiteOptions;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly ISiteStatisticsService _siteStatisticsService;

        public StaticSiteMiddleware(RequestDelegate next, 
            IOptions<StaticSiteOptions> staticSiteOptions, 
            ISiteService siteService, 
            IContentManager contentManager,
            ISiteStatisticsService siteStatisticsService)
        {
            _staticSiteOptions = staticSiteOptions;
            _siteService = siteService;
            _contentManager = contentManager;
            _siteStatisticsService = siteStatisticsService;
        }

        public async Task Invoke(HttpContext context)
        {            
            var routeValues = context.GetRouteData().Values;
            if (await HandleDashboardContent(context))
            {
                return;
            }

            string siteName = (string)routeValues["sitename"], sitePath = (string)routeValues["sitepath"];

            var siteInfo = await _siteService.GetSiteByNameAsync(siteName).ConfigureAwait(false);
            if (siteInfo == null)
            {
                throw new StaticSiteProcessingException(404, "Oops, no such site!", $"No site with name '{siteName}' found.");
            }

            if (!siteInfo.IsActive)
            {
                await AddErrorToEventLog(context, siteInfo.Id, "The site has been turned off.");
                
                throw new StaticSiteProcessingException(503, "Access Denied", "This site is not available.");
            }

            var siteContentPath = Path.Combine(_staticSiteOptions.Value.ContentPath, siteName);
            if (!Directory.Exists(siteContentPath))
            {
                var errMessage = $"No content folder found for site named '{siteName}'";
                await AddErrorToEventLog(context, siteInfo.Id, errMessage);
                
                throw new StaticSiteProcessingException(404, INVALID_SITE, errMessage);
            }

            var siteOwner = siteInfo.CreatedBy;
            if (siteOwner.Status != UserStatus.Active)
            {
                await AddErrorToEventLog(context, siteInfo.Id, "Site cannot be processed because of lock of its owner.");
                
                throw new StaticSiteProcessingException(400, INVALID_SITE, "This content cannot be provided because its owner has been blocked.");
            }

            var contentItems = await _contentManager.GetUploadedContentAsync(siteInfo.Id).ConfigureAwait(false);
            if (contentItems == null || !contentItems.Any())
            {
                await AddErrorToEventLog(context, siteInfo.Id, "Unable to find content for site.");
                
                throw new StaticSiteProcessingException(400, "Oops, no content", $"No content found for site named '{siteName}'");
            }

            var mappedSitePath = siteInfo.Mappings?.FirstOrDefault(m => m.Key == sitePath).Value;
            var fileName = mappedSitePath != null ? mappedSitePath : sitePath;
            var contentItem = contentItems.FirstOrDefault(ci => 
                  ci.FileName == fileName || $"{ci.DestinationPath.Replace('\\', '/')}/{ci.FileName}" == fileName
             );

            if (contentItem != null)
            {
                var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
                var fi = fileProvider.GetFileInfo(fileName);

                await CheckIfSiteIsViewed(fi, siteInfo);

                context.Response.ContentType = contentItem.ContentType;

                await context.Response.SendFileAsync(fi).ConfigureAwait(false);
                Console.WriteLine($"Content sent: {fileName}");
            }
            else
            {
                await AddErrorToEventLog(context, siteInfo.Id, $"Resource with name '{fileName}' was not found.");
                
                throw new StaticSiteProcessingException(404, "Oops, no content", $"No content with name {fileName} found.");
            }
        }

        private async ValueTask CheckIfSiteIsViewed(IFileInfo fi, Site siteInfo)
        {
            var landingPage = siteInfo.LandingPage;
            if (!string.IsNullOrEmpty(landingPage) && fi.Name == landingPage)
            {                
                await _siteStatisticsService.MarkSiteAsViewed(siteInfo.Id);
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
                var ctpProvider = new FileExtensionContentTypeProvider();
                string contentType;
                if (!ctpProvider.TryGetContentType(fi.Name, out contentType))
                {
                    contentType = "application/octet-stream";
                }
                
                context.Response.ContentType = contentType;
                await context.Response.SendFileAsync(fi);
                
                return true;
            }
            
            return false;
        }

        private async Task AddErrorToEventLog(HttpContext context, string siteId, string errorMessage)
        {
            var eventLogService = (IEventLogsService)context.RequestServices.GetService(typeof(IEventLogsService));
            await eventLogService.InsertSiteEventAsync(siteId, INVALID_SITE, Models.SiteEventType.Error, errorMessage);
        }
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