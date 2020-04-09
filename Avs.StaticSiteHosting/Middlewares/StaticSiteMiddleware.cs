using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
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
            if ((string)routeValues.FirstOrDefault().Value == "favicon.ico")
            {
                return;
            }

            var siteName = (string)routeValues["sitename"];
            var sitePath = routeValues["sitepath"];

            var siteContentPath = Path.Combine(staticSiteOptions.Value.ContentPath, siteName);
            if (!Directory.Exists(siteContentPath))
            {
                throw new FileNotFoundException($"Cannot find content for {siteName}.");
            }

            //TODO: should be taken from DB
            var fileName = (string)sitePath;
            var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
            var fi = fileProvider.GetFileInfo(fileName);

            await context.Response.SendFileAsync(fi);
            
            Console.WriteLine($"Content sent: {fileName}");
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