using Avs.Messaging.Contracts;
using Avs.Messaging.Core;
using Avs.Messaging.RabbitMq;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent
{
    public class SiteContentUpdatedConsumer(
        ISiteContentProvider contentProvider,
        IServiceProvider serviceProvider,
        ContentCacheService contentCacheService,
        ILogger<SiteContentUpdatedConsumer> logger) : ConsumerBase<ContentUpdatedEvent>
    {
        protected override async Task Consume(MessageContext<ContentUpdatedEvent> context)
        {
            var siteInfo = contentProvider.GetSiteContentById(context.Message.SiteId);
            
            if (siteInfo is null)
            {
                return;
            }

            string siteName = siteInfo.Name;
            using var scope = serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredKeyedService<IRpcClient>(RabbitMqOptions.TransportName);

            var response = await client.RequestAsync<GetSiteContentRequestMessage, SiteContentInfoResponse>(
                    new GetSiteContentRequestMessage { SiteName = siteName }
                );
            
            if (response.SiteContent is null)
            {
                logger.LogWarning("Unable to update configuration for site '{siteName}'", siteName);
                return;
            }

            await contentProvider.SetSiteConfig(response.SiteContent);

            foreach (var sitePath in siteInfo.GetSitePaths())
            {
                contentCacheService.RemoveCacheEntry(ContentCacheHelper.GetContentCacheKey(siteName, sitePath));
            }

            logger.LogInformation("Configuration updated successfully for site '{siteName}'", siteName);
        }
    }
}