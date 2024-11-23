using Avs.StaticSiteHosting.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Avs.StaticSiteHosting.Shared.Common;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds configure for StaticSiteOptions based on app settings and environment variables.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException">Throws an exception if all static site settings are not configured.</exception>
    public static IServiceCollection AddStaticSiteOptions(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddOptions<StaticSiteOptions>().Configure(options =>
        {
            var staticSiteSettingsSection = configuration.GetSection("StaticSiteOptions");
            var staticSiteOptions = staticSiteSettingsSection.Get<StaticSiteOptions>();
            
            var envContentPath = Environment.GetEnvironmentVariable("CONTENT_PATH");
            var envTempContentPath = Environment.GetEnvironmentVariable("TEMP_CONTENT_PATH");
            
            var contentPath = envContentPath ?? staticSiteOptions?.ContentPath ?? string.Empty;
            var tempContentPath = envTempContentPath ?? staticSiteOptions?.TempContentPath ?? string.Empty;
            
            ArgumentException.ThrowIfNullOrWhiteSpace(contentPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(tempContentPath);

            options.ContentPath = contentPath.Replace('\\', Path.DirectorySeparatorChar);
            options.TempContentPath = tempContentPath.Replace('\\', Path.DirectorySeparatorChar);
        });
        
        return services;
    }
}