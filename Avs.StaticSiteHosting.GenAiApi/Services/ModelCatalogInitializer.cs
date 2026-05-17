using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public class ModelCatalogInitializer(ILlmProvider modelCatalogProvider,
    IPayloadMappingProvider payloadMappingProvider,
    ILogger<ModelCatalogInitializer> logger): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // In memory data providers
            await modelCatalogProvider.LoadDataAsync();
            await payloadMappingProvider.LoadDataAsync();
            
            // Model catalog events registration
            await modelCatalogProvider.SubscribeToEventsAsync();
            
            // Mapping payload events registration
            await payloadMappingProvider.SubscribeToEventsAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error initializing model catalog");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}