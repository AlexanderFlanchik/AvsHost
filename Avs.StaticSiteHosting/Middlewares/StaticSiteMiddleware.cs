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
using Microsoft.Extensions.Logging;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Avs.StaticSiteHosting.Web.Common;
using Newtonsoft.Json;

namespace Avs.StaticSiteHosting.Web.Middlewares
{
    /// <summary>
    /// General middleware which serves static site content.
    /// </summary>
    public class StaticSiteMiddleware
    {
        private const string INVALID_SITE = "Invalid Site";
        private const string LOCAL_IP = "::1";
        private const string APPLICATION_OCTET_STREAM = "application/octet-stream";
        
        private readonly ILogger<StaticSiteMiddleware> _logger;
        private readonly IOptions<StaticSiteOptions> _staticSiteOptions;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly ISiteStatisticsService _siteStatisticsService;
        private readonly ICloudStorageProvider _cloudStorageProvider;
        private readonly ISettingsManager _settingsManager;
        private readonly IEventLogsService _eventLogsService;

        public StaticSiteMiddleware(RequestDelegate next, 
            ILogger<StaticSiteMiddleware> logger,
            IOptions<StaticSiteOptions> staticSiteOptions, 
            ISiteService siteService, 
            IContentManager contentManager,
            ISiteStatisticsService siteStatisticsService,
            ICloudStorageProvider cloudStorageProvider,
            ISettingsManager settingsManager,
            IEventLogsService eventLogsService)
        {
            _logger = logger;
            _staticSiteOptions = staticSiteOptions;
            _siteService = siteService;
            _contentManager = contentManager;
            _siteStatisticsService = siteStatisticsService;
            _cloudStorageProvider = cloudStorageProvider;
            _settingsManager = settingsManager;
            _eventLogsService = eventLogsService;
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
                await AddErrorToEventLog(siteInfo.Id, "The site has been turned off.");
                
                throw new StaticSiteProcessingException(503, "Access Denied", "This site is not available.");
            }

            var siteContentPath = Path.Combine(_staticSiteOptions.Value.ContentPath, siteName);
            if (!Directory.Exists(siteContentPath))
            {
                var errMessage = $"No content folder found for site named '{siteName}'";
                await AddErrorToEventLog(siteInfo.Id, errMessage);
                
                throw new StaticSiteProcessingException(404, INVALID_SITE, errMessage);
            }

            var siteOwner = siteInfo.CreatedBy;
            if (siteOwner.Status != UserStatus.Active)
            {
                await AddErrorToEventLog(siteInfo.Id, "Site cannot be processed because of lock of its owner.");
                
                throw new StaticSiteProcessingException(400, INVALID_SITE, "This content cannot be provided because its owner has been blocked.");
            }

            var contentItems = await _contentManager.GetUploadedContentAsync(siteInfo.Id).ConfigureAwait(false);
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

            if (contentItem != null)
            {
                var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
                var fi = fileProvider.GetFileInfo(fileName);
                
                if (!fi.Exists)
                {
                    // TODO: add caching for settings
                    var cloudSettingsEntry = await _settingsManager.GetAsync(CloudStorageSettings.SettingsName);
                    if (cloudSettingsEntry is null)
                    {
                        await NoCloudContentError();
                    }

                    var cloudSettings = JsonConvert.DeserializeObject<CloudStorageSettings>(cloudSettingsEntry.Value);
                    if (!cloudSettings.Enabled)
                    {
                        await NoCloudContentError();
                    }

                    // file does not exist in the application local storage, check it in the cloud bucket
                    using var cloudStream = await _cloudStorageProvider.GetCloudContent(siteInfo.CreatedBy.Name, siteName, fi.Name);
                    if (cloudStream is null)
                    {
                        await NoCloudContentError();
                    }

                    context.Response.ContentType = contentItem.ContentType;
                    await cloudStream.CopyToAsync(context.Response.Body);

                    return;

                    async Task NoCloudContentError()
                    {
                        await AddErrorToEventLog(siteInfo.Id, $"No content {fi.Name} for site {siteName}.");

                        throw new StaticSiteProcessingException(404, "Oops, no content", $"No content found for site named '{siteName}'");
                    }
                }

                await CheckIfSiteIsViewed(fi, siteInfo, context);

                context.Response.ContentType = contentItem.ContentType;

                await context.Response.SendFileAsync(fi).ConfigureAwait(false);
                _logger.LogInformation($"Content sent: {fileName}");
            }
            else
            {
                await AddErrorToEventLog(siteInfo.Id, $"Resource with name '{fileName}' was not found.");
                
                throw new StaticSiteProcessingException(404, "Oops, no content", $"No content with name {fileName} found.");
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
                var ctpProvider = new FileExtensionContentTypeProvider();
                string contentType;
                if (!ctpProvider.TryGetContentType(fi.Name, out contentType))
                {
                    contentType = APPLICATION_OCTET_STREAM;
                }
                
                context.Response.ContentType = contentType;
                await context.Response.SendFileAsync(fi);
                
                return true;
            }
            
            return false;
        }

        private Task AddErrorToEventLog(string siteId, string errorMessage)
                => _eventLogsService.InsertSiteEventAsync(siteId, INVALID_SITE, Models.SiteEventType.Error, errorMessage);
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