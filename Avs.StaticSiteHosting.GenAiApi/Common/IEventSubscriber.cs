using Avs.StaticSiteHosting.GenAiApi.Services;

namespace Avs.StaticSiteHosting.GenAiApi.Common;

/// <summary>
/// Represents a service that needs to act to some events
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes to event bus using <see cref="EventBusPubSub"/> instance
    /// </summary>
    /// <returns>A task which resolves when subscriptions are completed</returns>
    Task SubscribeToEventsAsync();
}