using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Avs.Messaging.Contracts;
using Avs.Messaging.Core;

namespace Avs.StaticSiteHosting.Web.Messaging.SiteEvents
{
    public class SiteVisitedConsumer(
        ISiteStatisticsService siteStatistics, 
        IHubContext<UserNotificationHub> hubContext) : ConsumerBase<SiteVisited>
    {
        private const string NEW_SITE_VISIT = "site-visited";
        
        protected override async Task Consume(MessageContext<SiteVisited> context)
        {
            var payload = context.Message;
            bool visited = await siteStatistics.MarkSiteAsViewed(payload.SiteId, payload.Visitor);
            if (!visited)
            {
                return;
            }

            await hubContext.Clients.User(payload.OwnerId).SendAsync(NEW_SITE_VISIT);
        }
    }
}