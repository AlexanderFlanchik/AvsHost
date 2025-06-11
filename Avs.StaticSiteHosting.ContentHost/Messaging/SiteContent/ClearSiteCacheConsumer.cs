using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;
using MassTransit;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;

public class ClearSiteCacheConsumer(ISiteContentProvider siteContentProvider) : IConsumer<ClearSiteCacheRequest>
{
    public Task Consume(ConsumeContext<ClearSiteCacheRequest> context)
    {
        siteContentProvider.ClearSiteCache(context.Message.Duration);
        return Task.CompletedTask;
    }
}