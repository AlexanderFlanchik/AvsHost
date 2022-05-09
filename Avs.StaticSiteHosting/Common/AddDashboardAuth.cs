using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Common
{
    public static class AddDashboardAuthServiceExtensions
    {
        public static IServiceCollection AddDashboardAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthSettings.ValidIssuer,
                        ValidateAudience = true,
                        ValidAudience = AuthSettings.ValidAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = AuthSettings.SecurityKey(),
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query[GeneralConstants.GET_ACCESS_TOKEN_NAME_SIGNALR]; // For SignalR
                            if (string.IsNullOrEmpty(accessToken))
                            {
                                accessToken = context.Request.Query[GeneralConstants.GET_ACCESS_TOKEN_NAME]; // For authorized GET REST methods
                            }

                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken.ToString();
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }
    }
}