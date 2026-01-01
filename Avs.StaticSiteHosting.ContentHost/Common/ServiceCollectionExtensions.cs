using Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;
using Avs.StaticSiteHosting.ContentHost.Middlewares;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Common;

namespace Avs.StaticSiteHosting.ContentHost.Common
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds site content services to DI container
        /// </summary>
        /// <param name="services">A service collection</param>
        /// <param name="configuration">App configuration</param>
        /// <returns></returns>
        public static IServiceCollection AddSiteContent(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ISiteContentProvider, SiteContentProvider>();
            services.AddSingleton<ISiteContentRequester, SiteContentRequester>();
            services.AddSingleton<SiteMiddleware>();
            services.AddSingleton<CustomRouteMiddleware>();
            services.AddSingleton<SiteContentMiddleware>();
            services.AddSingleton<ISiteEventPublisher, SiteEventPublisher>();
            services.AddSingleton<ISiteContentHandler, SiteContentHandler>();
            services.AddHostedService(sp => sp.GetRequiredService<ISiteEventPublisher>());
            services.AddSingleton<IErrorPageHandler, ErrorPageHandler>();
            services.AddSingleton<ContentCacheService>();
            services.AddHostedService(sp => sp.GetRequiredService<ContentCacheService>());
            services.AddHttpClient<CustomRouteHandlerApiClient>((_, client) =>
            {
                client.BaseAddress = new Uri(configuration.GetValue<string>("CustomRouteHandlerApiUrl")!);
            });
            
            return services;
        }

        public static IServiceCollection AddCloudStorage(this IServiceCollection services)
        {
            services.AddSingleton<ICloudStorageProvider, CloudStorageProvider>();
            services.AddSingleton<CloudStorageSettings>();
            services.AddSingleton<ICloudStorageSettingsProvider, CloudStorageSettingsProvider>();
            services.AddSingleton<CloudStorageWorker>();
            services.AddHostedService(sp => sp.GetRequiredService<CloudStorageWorker>());

            return services;
        }
    }
}