using System.Collections.Concurrent;
using System.Text.Json;
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

    /// <summary>
    /// Returns a mapping list if found by mapping ID, or empty collection otherwise
    /// </summary>
    /// <param name="mapingId">Mapping ID</param>
    /// <returns>A string value or null</returns>
    Mapping[] GetMappings(string mapingId);
}

public class PayloadMappingProvider(
    IServiceScopeFactory scopeFactory,
    EventBusPubSub pubSub,
    ILogger<PayloadMappingProvider> logger) : IPayloadMappingProvider
{
    private readonly ConcurrentDictionary<string, Mapping[]> _mappings = new();
    
    public async Task LoadDataAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayloadMappingRepository>();
        var mappings = await repository.GetAllMappingsAsync();

        foreach (var mapping in mappings)
        {
            try
            {
                var mappingList = JsonSerializer.Deserialize<Mapping[]>(mapping.JsonContent);
                if (mappingList is null || mappingList.Length == 0)
                {
                    _mappings[mapping.Id] = [];
                    continue;
                }
                
                _mappings[mapping.Id] = mappingList;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error loading payload mapping with ID = {id}", mapping.Id);
            }
        }
        
        logger.LogInformation("Loaded {mappingCount} vendor payload mapping(s)", _mappings.Count);
    }

    public Mapping[] GetMappings(string mappingId)
    {
        return _mappings.GetValueOrDefault(mappingId) ?? [];
    }

    public async Task SubscribeToEventsAsync()
    {
        await pubSub.SubscribeAsync<MappingPayloadChanged>(On);
        await pubSub.SubscribeAsync<MappingDeleted>(On);
    }

    private void On(MappingPayloadChanged @event)
    {
        try
        {
            var newMappings = JsonSerializer.Deserialize<Mapping[]>(@event.JsonContent);
            _mappings[@event.MappingId] = newMappings ?? [];
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading payload mapping");
        }
    }

    private void On(MappingDeleted @event)
    {
        _ = _mappings.TryRemove(@event.MappingId, out _);
    }
}