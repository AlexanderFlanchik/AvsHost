using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;
using MassTransit;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent
{
    public class SiteContentUpdatedConsumer(ISiteContentProvider contentProvider) : IConsumer<ContentUpdatedEvent>
    {
        public async Task Consume(ConsumeContext<ContentUpdatedEvent> context)
        {
            var siteInfo = context.Message.SiteContent;
            var siteExists = contentProvider.GetSitesList().Any(s => s == siteInfo.Name);
            
            if (!siteExists)
            {
                return;
            }

            await contentProvider.SetSiteConfig(siteInfo);
        }
    }
}