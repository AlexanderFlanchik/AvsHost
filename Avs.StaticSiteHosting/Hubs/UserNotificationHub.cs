using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Avs.StaticSiteHosting.Web.Hubs
{
    [Authorize]
    public class UserNotificationHub : Hub
    {
    }
}