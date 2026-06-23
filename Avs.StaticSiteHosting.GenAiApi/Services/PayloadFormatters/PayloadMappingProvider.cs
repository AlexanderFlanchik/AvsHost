using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Repositories;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public interface IPayloadMappingProvider : IEventSubscriber
{
    /// <summary>
    /// Loads vendor mappings from the database.
    /// </summary>
    /// <returns>A task which completes when vendor mappings are loaded</returns>
    Task LoadDataAsync();
}

public class PayloadMappingProvider(
    IServiceScopeFactory scopeFactory,
    IJsonRemapper jsonRemapper,
    EventBusPubSub pubSub,
    ILogger<PayloadMappingProvider> logger) : IPayloadMappingProvider
{
    public async Task LoadDataAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayloadMappingRepository>();
        var mappings = await repository.GetAllMappingsAsync();

        int i = 0;
        foreach (var mapping in mappings)
        {
            try
            {
                jsonRemapper.RegisterFromJson(mapping.JsonContent, mapping.Id);
                i++;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error loading payload mapping with ID = {id}", mapping.Id);
            }
        }
        
        logger.LogInformation("Loaded {mappingCount} vendor payload mapping(s)", i);
    }
    
    public async Task SubscribeToEventsAsync()
    {
        await pubSub.SubscribeAsync<MappingPayloadChanged>(On);
        await pubSub.SubscribeAsync<MappingDeleted>(On);
    }

    private void On(MappingPayloadChanged @event)
    {
        jsonRemapper.RegisterFromJson(@event.JsonContent, @event.MappingId);
    }

    private void On(MappingDeleted @event)
    {
        jsonRemapper.Unregister(@event.MappingId);
    }
}