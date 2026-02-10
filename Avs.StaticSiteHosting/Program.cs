using System;
using Avs.Messaging;
using Avs.Messaging.RabbitMq;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Avs.StaticSiteHosting.Web;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Middlewares;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Messaging.SiteContent;
using Avs.StaticSiteHosting.Web.Messaging.SiteEvents;
using Avs.StaticSiteHosting.Web.Messaging.SettingsProvider;
using Microsoft.Extensions.Configuration;
using Avs.StaticSiteHosting.Web.DTOs;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSession(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.AddMongoDBClient("mongo");
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddSignalRServices();
builder.Services.AddIdentityServices();
builder.Services.AddContentManagement();
builder.Services.AddReports();

builder.Services.AddUserHelpModule();
builder.Services.AddConversationModule();

builder.Services.AddEventsAndLogs();
builder.Services.AddDashboardAuthentication();

builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddContentEditor();
builder.Services.AddScoped<ResourcePreviewContentMiddleware>();

builder.Services.AddMessaging(x =>
{
    x.AddConsumer<SiteErrorConsumer>();
    x.AddConsumer<SiteContentRequestConsumer>();
    x.AddConsumer<CloudSettingsRequestConsumer>();
    x.AddConsumer<SiteVisitedConsumer>();
    
    x.UseRabbitMq(cfg =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQSettings").Get<RabbitMqSettings>()!;
        cfg.Host = rabbitMqSettings.Host;
        cfg.Port = rabbitMqSettings.Port;
        
        cfg.ConfigureRequestReply<GetSiteContentRequestMessage, SiteContentInfoResponse>();
        cfg.ConfigureRequestReply<CloudSettingsRequest, CloudSettingsResponse>();
        cfg.ConfigureExchangeOptions<SiteVisited>(o => o.ExchangeName = "SiteVisited");
        cfg.ConfigureExchangeOptions<CustomRouteHandlerChanged>(o => o.IsExchangeDurable = false);
        cfg.ConfigureExchangeOptions<CustomRouteHandlersDeleted>(o => o.IsExchangeDurable = false);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseMiddleware<ResourcePreviewContentMiddleware>();
app.UseRouting();

app.UseSession();
app.UseDashboard();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/well-known/config", (IConfiguration config) => 
    Results.Ok(
        new ConfigModel()
        {
            ContentHostUrl = Environment.GetEnvironmentVariable("CONTENT_HOST_URL") 
                         ?? config["ContentHostUrl"]
        })
    );

app.MapControllers();
app.MapHub<UserNotificationHub>("/user-notification");

app.Run();