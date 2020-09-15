using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Avs.StaticSiteHosting.Models.Identity;
using Avs.StaticSiteHosting.Services;
using Avs.StaticSiteHosting.Services.ContentManagement;

namespace Avs.StaticSiteHosting.Middlewares
{
    /// <summary>
    /// General middleware which serves static site content.
    /// </summary>
    public class StaticSiteMiddleware
    {
        public StaticSiteMiddleware(RequestDelegate next) {}

        public async Task Invoke(HttpContext context, IOptions<StaticSiteOptions> staticSiteOptions, ISiteService siteService, IContentManager contentManager)
        {
            var routeValues = context.GetRouteData().Values;
            if (await HandleDashboardContent(context))
            {
                return;
            }

            string siteName = (string)routeValues["sitename"], sitePath = (string)routeValues["sitepath"];

            var siteInfo = await siteService.GetSiteByNameAsync(siteName).ConfigureAwait(false);
            if (siteInfo == null)
            {
                throw new StaticSiteProcessingException(404, "Oops, no such site!", $"No site with name '{siteName}' found.");
            }

            if (!siteInfo.IsActive)
            {
                throw new StaticSiteProcessingException(503, "Access Denied", "This site is not available.");
            }

            var siteContentPath = Path.Combine(staticSiteOptions.Value.ContentPath, siteName);
            if (!Directory.Exists(siteContentPath))
            {
                throw new StaticSiteProcessingException(404, "Invalid Site",  $"No content folder found for site named '{siteName}'");
            }

            var siteOwner = siteInfo.CreatedBy;
            if (siteOwner.Status != UserStatus.Active)
            {
                throw new StaticSiteProcessingException(400, "Invalid Site", "This content cannot be provided because its owner has been blocked.");
            }

            var contentItems = await contentManager.GetUploadedContentAsync(siteInfo.Id).ConfigureAwait(false);
            if (contentItems == null || !contentItems.Any())
            {
                throw new StaticSiteProcessingException(400, "Oops, no content", $"No content found for site named '{siteName}'");
            }

            var resourceMappings = siteInfo.Mappings;
            var mappedSitePath = resourceMappings?.FirstOrDefault(m => m.Key == sitePath).Value;
            var fileName = mappedSitePath != null ? mappedSitePath : sitePath;
            var contentItem = contentItems.FirstOrDefault(ci => 
                  ci.FileName == fileName || $"{ci.DestinationPath.Replace('\\', '/')}/{ci.FileName}" == fileName
             );

            if (contentItem != null)
            {
                var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
                var fi = fileProvider.GetFileInfo(fileName);

                await context.Response.SendFileAsync(fi).ConfigureAwait(false);
                Console.WriteLine($"Content sent: {fileName}");
            }
            else
            {
                throw new StaticSiteProcessingException(404, "Oops, no content", $"No content with name {fileName} found.");
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
                await context.Response.SendFileAsync(fi);
                return true;
            }
            
            return false;
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