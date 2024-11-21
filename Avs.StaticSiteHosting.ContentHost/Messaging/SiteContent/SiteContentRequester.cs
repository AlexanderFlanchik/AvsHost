using Avs.StaticSiteHosting.Shared.Contracts;
using MassTransit;

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
            var client = scope.ServiceProvider.GetRequiredService<IRequestClient<GetSiteContentRequestMessage>>();

            var response = await client.GetResponse<SiteContentInfoResponse>(request);

            return response.Message.SiteContent;
        }
    }
}