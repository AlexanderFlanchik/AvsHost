using Avs.Messaging.Contracts;
using Avs.Messaging.Core;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;

public class SiteContentInfoResponseConsumer : ConsumerBase<SiteContentInfoResponse>
{
    protected override Task Consume(MessageContext<SiteContentInfoResponse> messageContext)
    {
        return Task.CompletedTask;
    }
}