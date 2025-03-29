using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Avs.StaticSiteHosting.DataMigrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder();
            
            builder.AddMongoDBClient("mongo");
            builder.Services.AddTransient<PasswordHasher>();
            builder.Services.AddSingleton<MongoEntityRepository>();
            builder.Services.AddSingleton<AppInitializer>();
            builder.Services.AddSingleton<DbInitializer>();
            builder.Services.AddSingleton<HelpContentInitializer>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<AppInitializer>());

            var app = builder.Build();

            await app.RunAsync();
        }
    }
}