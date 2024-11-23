using Avs.StaticSiteHosting.ContentHost.Configuration;
using Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;
using Avs.StaticSiteHosting.ContentHost.Middlewares;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Common;

namespace Avs.StaticSiteHosting.ContentHost.Common
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStaticSiteOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<StaticSiteOptions>().Configure(options =>
            {
                var staticSiteSettingsSection = configuration.GetSection("StaticSiteOptions");
                var staticSiteOptions = staticSiteSettingsSection.Get<StaticSiteOptions>() 
                    ?? throw new InvalidOperationException("No static site options configured. Please configure.");

                var envContentPath = Environment.GetEnvironmentVariable("CONTENT_PATH");
                var envTempContentPath = Environment.GetEnvironmentVariable("TEMP_CONTENT_PATH");

                options.ContentPath = (!string.IsNullOrEmpty(envContentPath) ? envContentPath
                    : staticSiteOptions.ContentPath)?.Replace('\\', Path.DirectorySeparatorChar)!;
                
                options.TempContentPath = (!string.IsNullOrEmpty(envTempContentPath) ? envContentPath
                    : staticSiteOptions.TempContentPath)?.Replace('\\', Path.DirectorySeparatorChar)!;
            });

            return services;
        }

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
            services.AddSingleton<SiteContentMiddleware>();
            services.AddSingleton<ISiteEventPublisher, SiteEventPublisher>();
            services.AddHostedService(sp => sp.GetRequiredService<ISiteEventPublisher>());

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