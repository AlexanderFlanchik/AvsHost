﻿using Avs.StaticSiteHosting.ContentHost.Configuration;
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
            services.Configure<StaticSiteOptions>(configuration.GetSection("StaticSiteOptions"));
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