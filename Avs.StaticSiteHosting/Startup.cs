using Avs.StaticSiteHosting.Web.Middlewares;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

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
            
            services.AddTransient<PasswordHasher>();
            services.AddSingleton<MongoEntityRepository>();

            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            
            services.AddScoped<ISiteService, SiteService>();
            services.AddScoped<IContentManager, ContentManager>();
            
            services.AddScoped<IHelpContentService, HelpContentService>();
            services.AddScoped<IHelpResourceService, HelpResourceService>();
            
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
                        IssuerSigningKey = AuthSettings.SecurityKey()
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
            app.Use(async (ctx, next) =>
            {
                if (ctx.Request.Method == "GET" && 
                    ctx.Request.Query.TryGetValue(GeneralConstants.GET_ACCESS_TOKEN_NAME, out var tokenVal))
                {
                    var accessToken = tokenVal.ToString();
                    ctx.Request.Headers["authorization"] = $"Bearer {accessToken}";
                }
                await next();
            });

            app.UseDashboard();
            
            // Auth is only required for dashboard Web API
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapStaticSite("/{sitename:required}/{**sitepath}");
                endpoints.MapControllers();
            });
        }
    }
}