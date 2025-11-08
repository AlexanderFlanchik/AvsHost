using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Services;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.Databases;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Avs.StaticSiteHosting.Web.Services.Reporting;
using Avs.StaticSiteHosting.Web.Services.Reporting.SiteErrors;
using Avs.StaticSiteHosting.Web.Services.Reporting.SiteEvents;
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

    public static class CoreServicesExtensions 
    { 
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStaticSiteOptions(configuration);

            services.AddSingleton<StorageInitializer>();
            services.AddHostedService(sp => sp.GetRequiredService<StorageInitializer>());
            services.AddTransient<PasswordHasher>();
            services.AddSingleton<MongoEntityRepository>();
            services.AddSingleton<ISettingsManager, SettingsManager>();

            services.AddSingleton<CloudStorageSettings>();
            services.AddHostedService<CloudStorageSettingsWorker>();
            services.AddSingleton<CloudStorageSyncWorker>();
            services.AddHostedService(sp => sp.GetRequiredService<CloudStorageSyncWorker>());
            services.AddScoped<ITagsService, TagsService>();
            services.AddScoped<ITagSiteService, TagSiteService>();

            return services;
        }
    }

    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
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
            services.AddScoped<ISiteManagementService, SiteManagementService>();
            services.AddScoped<IContentUploadService, ContentUploadService>();
            services.AddScoped<IPagePreviewService, PagePreviewService>();
            services.AddSingleton<ICloudStorageProvider, CloudStorageProvider>();
            services.AddTransient<ImageResizeService>();
            services.AddScoped<IDatabaseService, DatabaseService>();

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

    public static class ReportExtensions
    {
        public static IServiceCollection AddReports(this IServiceCollection services) 
        {
            // Sites General
            services.AddScoped<IReportDataService, SitesGeneralReportDataService>();

            // Site Events
            services.AddScoped<IReportDataService, SiteEventsReportDataService>();

            // Site visits
            services.AddScoped<IReportDataService, SiteVisitsStatisticsReportDataService>();

            // Site errors
            services.AddScoped<IReportDataService, SiteErrorsStatisticsReportDataService>();

            // All report services
            services.AddScoped<IReportProvider, ReportProvider>()
                    .AddScoped<IReportingService, ReportService>()
                    .AddScoped<IReportDataFacade, ReportDataFacade>();

            // Report renderers
            services.AddScoped<IReportRenderer, PdfReportRenderer>()
                    .AddScoped<IReportRenderer, XlsxReportRenderer>();

            return services;
        }
    }
}