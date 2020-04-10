using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Middlewares
{
    /// <summary>
    /// General middleware which serves static site content.
    /// </summary>
    public class StaticSiteMiddleware
    {
        public StaticSiteMiddleware(RequestDelegate next) {}

        public async Task Invoke(HttpContext context, IOptions<StaticSiteOptions> staticSiteOptions, MongoEntityRepository mongoRepository)
        {
            var routeValues = context.GetRouteData().Values;
            if (await HandleDashboardContent(context))
            {
                return;
            }

            var siteName = (string)routeValues["sitename"];
            var sitePath = routeValues["sitepath"];

            var siteContentPath = Path.Combine(staticSiteOptions.Value.ContentPath, siteName);
            if (!Directory.Exists(siteContentPath))
            {
                return;
            }

            //TODO: should be taken from DB
            var fileName = (string)sitePath;
            var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
            var fi = fileProvider.GetFileInfo(fileName);

            await context.Response.SendFileAsync(fi);
            
            Console.WriteLine($"Content sent: {fileName}");
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