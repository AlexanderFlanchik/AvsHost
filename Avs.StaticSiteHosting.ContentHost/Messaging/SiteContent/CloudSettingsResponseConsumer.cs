using Avs.Messaging.Contracts;
using Avs.Messaging.Core;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;

public class CloudSettingsResponseConsumer : ConsumerBase<CloudSettingsResponse>
{
    protected override Task Consume(MessageContext<CloudSettingsResponse> messageContext)
    {
        return Task.CompletedTask;
    }
}