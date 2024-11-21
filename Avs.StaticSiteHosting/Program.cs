using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Avs.StaticSiteHosting.Web;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Middlewares;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Web.Messaging.SiteContent;
using Avs.StaticSiteHosting.Web.Messaging.SiteEvents;
using Avs.StaticSiteHosting.Web.Messaging.SettingsProvider;
using Microsoft.Extensions.Configuration;
using Avs.StaticSiteHosting.Web.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSession(options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

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
builder.Services.AddMessaging(builder.Configuration, options =>
{
    options.AddConsumer<SiteErrorConsumer>();
    options.AddConsumer<SiteContentRequestConsumer>();
    options.AddConsumer<CloudSettingsRequestConsumer>();
    options.AddConsumer<SiteVisitedConsumer>();
});

var app = builder.Build();

if (!builder.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        new ExceptionHandlerOptions() 
        { 
            AllowStatusCode404Response = true, 
            ExceptionHandlingPath = $"/{GeneralConstants.ERROR_ROUTE}" 
        });
}

app.UseMiddleware<ResourcePreviewContentMiddleware>();
app.UseRouting();

app.UseSession();
app.UseDashboard();

// Auth is only required for dashboard Web API
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticSite("/{sitename:required}/{**sitepath}");
app.MapControllers();
app.MapGet("/well-known/config", (IConfiguration config) => {
    return Results.Ok(
        new ConfigModel() 
        { 
            ContentHostUrl = config["ContentHostUrl"] 
        });
});

app.MapHub<UserNotificationHub>("/user-notification");

var staticSiteOptions = (IOptions<StaticSiteOptions>)app.Services.GetService(typeof(IOptions<StaticSiteOptions>));
if (staticSiteOptions == null || staticSiteOptions.Value == null || string.IsNullOrEmpty(staticSiteOptions.Value.ContentPath))
{
    throw new Exception("Invalid application configuration. Static site options were not found or configured.");
}

InitStorage(staticSiteOptions.Value.ContentPath);
InitStorage(staticSiteOptions.Value.TempContentPath);

app.Run();

static void InitStorage(string contentPath)
{
    if (!Directory.Exists(contentPath))
    {
        Directory.CreateDirectory(contentPath);
        Console.WriteLine($"{contentPath} has been created.");
    }
    else
    {
        Console.WriteLine($"{contentPath} found.");
    }
}