using Avs.Messaging.Contracts;
using Avs.Messaging.Core;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;

public class ClearSiteCacheConsumer(ISiteContentProvider siteContentProvider) : ConsumerBase<ClearSiteCacheRequest>
{
    protected override Task Consume(MessageContext<ClearSiteCacheRequest> messageContext)
    {
        siteContentProvider.ClearSiteCache(messageContext.Message.Duration);
        return Task.CompletedTask;
    }
}