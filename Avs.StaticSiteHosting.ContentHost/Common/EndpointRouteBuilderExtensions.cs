using Avs.StaticSiteHosting.ContentHost.Middlewares;
using Microsoft.Extensions.FileProviders;

namespace Avs.StaticSiteHosting.ContentHost.Common
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapSiteContent(this IEndpointRouteBuilder endpoints, string pattern) 
        {
            var applicationBuilder = endpoints.CreateApplicationBuilder()
                .UseMiddleware<SiteContentMiddleware>()
                .Build();

            return endpoints.Map(pattern, applicationBuilder);
        }

        public static IEndpointConventionBuilder MapCommonEndpoints(this IEndpointRouteBuilder endpoints)
        {
            RouteGroupBuilder group = endpoints.MapGroup("/");
            group.MapGet("/favicon.ico", () => Results.NotFound());
            group.MapGet("/", () => "Content server is up and running...");
            group.MapGet("/styles.css", () =>
            {
                var fileProvider = new PhysicalFileProvider(new DirectoryInfo("wwwroot").FullName);
                IFileInfo? fi = fileProvider.GetFileInfo("styles.css");
   
                return !fi.Exists ? Results.NotFound() : Results.File(fi.PhysicalPath!,"text/css");
            });
            
            return group;
        }
    }
}