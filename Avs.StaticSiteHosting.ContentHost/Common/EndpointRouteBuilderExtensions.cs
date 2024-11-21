using Avs.StaticSiteHosting.ContentHost.Middlewares;

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
    }
}