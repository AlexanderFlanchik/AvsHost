using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Avs.Messaging.Contracts;
using Avs.Messaging.Core;

namespace Avs.StaticSiteHosting.Web.Messaging.SiteEvents
{
    public class SiteErrorConsumer(IHubContext<UserNotificationHub> hubContext, IServiceProvider serviceProvider) : ConsumerBase<SiteError>
    {
        private const string INVALID_SITE = "Invalid Site";
        private const string SITE_ERROR_EVENT = "site-error";

        protected override async Task Consume(MessageContext<SiteError> context)
        {
            using var scope = serviceProvider.CreateScope();
            var eventLogsService = scope.ServiceProvider.GetService<IEventLogsService>();
            var errorInfo = context.Message;

            await eventLogsService.InsertSiteEventAsync(errorInfo.Id, INVALID_SITE, Models.SiteEventType.Error, errorInfo.Error);
            await hubContext.Clients.User(errorInfo.SiteOwnerId!).SendAsync(SITE_ERROR_EVENT);
        }
    }
}