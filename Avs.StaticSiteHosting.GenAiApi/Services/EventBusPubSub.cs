using System.Text.Json;
using StackExchange.Redis;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public class EventBusPubSub(
    IConnectionMultiplexer redis,
    ILogger<EventBusPubSub> logger)
{
    private readonly ISubscriber _subscriber = redis.GetSubscriber();
    private const string _eventBusChannel = "genaiapi-events";
    
    public async Task SubscribeAsync<T>(Action<T> handler)
    {
        var eventName = typeof(T).Name;
        var channel = RedisChannel.Pattern($"{_eventBusChannel}:{eventName}");
        
        await _subscriber.SubscribeAsync(channel, (_, message) =>
        {
            if (!message.HasValue)
            {
                return;
            }
            
            try
            {
                var payload = message.ToString();
                var @event = JsonSerializer.Deserialize<T>(payload)!;
                handler(@event);
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Error while parsing {eventName} payload.");
            }
        });
    }
    
    public async Task PublishAsync<T>(T @event)
    {
        var eventName = typeof(T).Name;
        var channel = RedisChannel.Pattern($"{_eventBusChannel}:{eventName}");
        var payload = JsonSerializer.Serialize(@event);
        
        await _subscriber.PublishAsync(channel, payload);
    }
}