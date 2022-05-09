using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Avs.StaticSiteHosting.Web;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Middlewares;
using Avs.StaticSiteHosting.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddSignalRServices();
builder.Services.AddIdentityServices();
builder.Services.AddContentManagement();

builder.Services.AddUserHelpModule();
builder.Services.AddConversationModule();

builder.Services.AddEventsAndLogs();
builder.Services.AddDashboardAuthentication();

builder.Services.AddControllersWithViews().AddNewtonsoftJson();

var app = builder.Build();

if (!builder.Environment.IsDevelopment())
{
    app.UseExceptionHandler($"/{GeneralConstants.ERROR_ROUTE}");
}

app.UseRouting();

app.UseDashboard();

// Auth is only required for dashboard Web API
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapStaticSite("/{sitename:required}/{**sitepath}");
    endpoints.MapControllers();
    endpoints.MapHub<UserNotificationHub>("/user-notification");
});

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