using Microsoft.AspNetCore.SignalR;

namespace Avs.StaticSiteHosting.Web.Common
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            return connection?.User?.FindFirst(AuthSettings.UserIdClaim)?.Value;
        }
    }
}