using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Middlewares;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web
{
    public class Startup
    {
        public IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<StaticSiteOptions>(Configuration.GetSection("StaticSiteOptions"));
            services.Configure<MongoDbSettings>(Configuration.GetSection("MongoDbConnection"));
            
            services.AddSignalR();
            services.AddSingleton<IUserIdProvider, UserIdProvider>();

            services.AddTransient<PasswordHasher>();
            services.AddSingleton<MongoEntityRepository>();

            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRoleService, UserRoleService>();

            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IContentManager, ContentManager>();
            
            services.AddScoped<IHelpContentService, HelpContentService>();
            services.AddScoped<IHelpResourceService, HelpResourceService>();

            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<IConversationMessagesService, ConversationMessagesService>();

            services.AddTransient<ImageResizeService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

            services.AddControllersWithViews().AddNewtonsoftJson();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
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
        }
    }
}