using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Avs.StaticSiteHosting.Web.Services.Sites;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Avs.StaticSiteHosting.Web.Common
{
    public static class SignalRServicesCollectionExtensions
    {
        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, UserIdProvider>();

            return services;
        }
    }

    public static class CoreServicesExternsions 
    { 
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StaticSiteOptions>(configuration.GetSection("StaticSiteOptions"));
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbConnection"));
            services.AddTransient<PasswordHasher>();
            services.AddSingleton<MongoEntityRepository>();
            services.AddSingleton<ISettingsManager, SettingsManager>();

            services.AddSingleton<CloudStorageSettings>();
            services.AddHostedService<CloudStorageSettingsWorker>();
            services.AddSingleton<CloudStorageSyncWorker>();
            services.AddHostedService(sp => sp.GetRequiredService<CloudStorageSyncWorker>());

            return services;
        }
    }

    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRoleService, UserRoleService>();

            return services;
        }
    }

    public static class ContentManagementServicesExtensions
    {
        public static IServiceCollection AddContentManagement(this IServiceCollection services)
        {
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IContentManager, ContentManager>();
            services.AddScoped<IContentUploadService, ContentUploadService>();
            services.AddScoped<IPagePreviewService, PagePreviewService>();
            services.AddSingleton<ICloudStorageProvider, CloudStorageProvider>();
            services.AddTransient<ImageResizeService>();

            return services;
        }
    }

    public static class UserHelpServicesExtensions
    {
        public static IServiceCollection AddUserHelpModule(this IServiceCollection services)
        {
            return services.AddScoped<IHelpContentService, HelpContentService>()
                            .AddScoped<IHelpResourceService, HelpResourceService>();
        }
    }

    public static class ConversationServicesExtensions
    {
        public static IServiceCollection AddConversationModule(this IServiceCollection services)
        {
            return services.AddScoped<IConversationService, ConversationService>()
                        .AddScoped<IConversationMessagesService, ConversationMessagesService>();
        }
    }

    public static class EventsAndLogsServicesExtensions
    {
        public static IServiceCollection AddEventsAndLogs(this IServiceCollection services)
        {
            services.AddScoped<IEventLogsService, EventLogsService>();
            services.AddScoped<ISiteStatisticsService, SiteStatisticsService>();
            services.AddScoped<IErrorSitesListService, ErrorSitesListService>();

            return services;
        }
    }

    public static class ContentEditorExtensions
    {
        public static IServiceCollection AddContentEditor(this IServiceCollection services)
        {
            services.AddScoped<IPageRenderingService, PageRenderingService>();
            return services;
        }
    }
}