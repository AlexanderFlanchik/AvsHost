using System.Text.Json;
using Avs.StaticSiteHosting.GenAiApi.Middlewares;
using Avs.StaticSiteHosting.GenAiApi.Repositories;
using Avs.StaticSiteHosting.GenAiApi.Services;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

namespace Avs.StaticSiteHosting.GenAiApi.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModelCatalog(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IModelRepository, ModelRepository>();
        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IModelService, ModelService>();
        
        services.AddSingleton<ILlmProvider, LlmProvider>();
        services.AddSingleton<EventBusPubSub>();
        services.AddSingleton<LlmRequestProxyMiddleware>();
        
        services.AddHostedService<ModelCatalogInitializer>();
        services.AddHttpClient<IProxyClient, DefaultProxyClient>((_, client) =>
        {
            client.Timeout = TimeSpan.FromSeconds(180);
        });
        
        return services;
    }

    public static IServiceCollection AddPayloadMappings(this IServiceCollection services,  JsonWriterOptions? writerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddScoped<IPayloadMappingRepository, PayloadMappingRepository>();
        services.AddSingleton<IPayloadMappingProvider, PayloadMappingProvider>();
        services.AddScoped<IPayloadMappingService, PayloadMappingService>();
        services.AddSingleton<IRequestTransform, RequestTransform>();
        services.AddSingleton<IResponseTransform, ResponseTransform>();

        services.AddSingleton<IJsonRemapper>(_ => new JsonRemapper() { WriterOptions = writerOptions ?? new JsonWriterOptions() });
        
        return services;
    }
}