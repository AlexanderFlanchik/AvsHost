using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Avs.StaticSiteHosting.Web;
using Microsoft.AspNetCore.SignalR;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;
using Avs.StaticSiteHosting.Web.Services.Sites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Avs.StaticSiteHosting.Web.Middlewares;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Services.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<StaticSiteOptions>(builder.Configuration.GetSection("StaticSiteOptions"));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbConnection"));

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

builder.Services.AddTransient<PasswordHasher>();
builder.Services.AddSingleton<MongoEntityRepository>();

builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<IContentManager, ContentManager>();

builder.Services.AddScoped<IHelpContentService, HelpContentService>();
builder.Services.AddScoped<IHelpResourceService, HelpResourceService>();

builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IConversationMessagesService, ConversationMessagesService>();

builder.Services.AddScoped<IEventLogsService, EventLogsService>();

builder.Services.AddScoped<ISiteStatisticsService, SiteStatisticsService>();
builder.Services.AddScoped<IErrorSitesListService, ErrorSitesListService>();
builder.Services.AddScoped<ISettingsManager, SettingsManager>();

builder.Services.AddTransient<ImageResizeService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
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