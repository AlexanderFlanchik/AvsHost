using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;
using MassTransit;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent
{
    public class SiteContentUpdatedConsumer(
        ISiteContentProvider contentProvider,
        IServiceProvider serviceProvider,
        ContentCacheService contentCacheService,
        ILogger<SiteContentUpdatedConsumer> logger) : IConsumer<ContentUpdatedEvent>
    {
        public async Task Consume(ConsumeContext<ContentUpdatedEvent> context)
        {
            var siteInfo = contentProvider.GetSiteContentById(context.Message.SiteId);
            
            if (siteInfo is null)
            {
                return;
            }

            string siteName = siteInfo.Name;
            using var scope = serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<GetSiteContentRequestMessage>>();

            var response = await client.GetResponse<SiteContentInfoResponse>(new GetSiteContentRequestMessage { SiteName = siteName });
            if (response.Message is null)
            {
                logger.LogWarning("Unable to update configuration for site '{siteName}'", siteName);
                return;
            }

            await contentProvider.SetSiteConfig(response.Message.SiteContent);

            foreach (var sitePath in siteInfo.GetSitePaths())
            {
                contentCacheService.RemoveCacheEntry(ContentCacheHelper.GetContentCacheKey(siteName, sitePath));
            }

            logger.LogInformation("Configuration updated successfully for site '{siteName}'", siteName);
        }
    }
}