using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Avs.StaticSiteHosting.Web
{
    public class AuthSettings
    {
        public const string UserIdClaim = "user-id";
        public const string ValidIssuer = "StaticSiteHostingDashboardApi";
        public const string ValidAudience = "StaticSiteHostingDashboardSpa";
        public static SymmetricSecurityKey SecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Secret@ForDashboard!123"));
        }

        public readonly static TimeSpan TokenLifetime = TimeSpan.FromMinutes(60);
    }
}
