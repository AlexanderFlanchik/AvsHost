using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.Messaging.Contracts;
using Avs.Messaging.RabbitMq;

namespace Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent
{
    public interface ISiteContentRequester
    {
        /// <summary>
        /// Gets a site content required
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns></returns>
        Task<SiteContentInfo?> RequestSiteContentAsync(string siteName);
    }

    public class SiteContentRequester(IServiceProvider serviceProvider) : ISiteContentRequester
    {
        public async Task<SiteContentInfo?> RequestSiteContentAsync(string siteName)
        {
            var request = new GetSiteContentRequestMessage
            { 
                SiteName = siteName 
            };

            using var scope = serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetRequiredKeyedService<IRpcClient>(RabbitMqOptions.TransportName);

            var response = await client.RequestAsync<GetSiteContentRequestMessage, SiteContentInfoResponse>(request);

            return response.SiteContent;
        }
    }
}