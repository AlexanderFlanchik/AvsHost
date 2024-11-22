using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace Avs.StaticSiteHosting.DataMigrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dbConnection = await File.ReadAllTextAsync("db_connection.json");
            var mongoDbSettings = JsonConvert.DeserializeObject<MongoDbSettings>(dbConnection);
            var host = new HostBuilder().ConfigureServices((_, services) =>
            {
                services.Configure<MongoDbSettings>(opt => {
                    opt.Database = mongoDbSettings.Database;
                    opt.Host = mongoDbSettings.Host;
                });

                services.AddTransient<PasswordHasher>();
                services.AddSingleton<MongoEntityRepository>();
                services.AddSingleton<AppInitializer>();
                services.AddSingleton<DbInitializer>();
                services.AddSingleton<HelpContentInitializer>();
                services.AddHostedService(sp => sp.GetRequiredService<AppInitializer>());
            }).Build();
            
            await host.RunAsync();
        }
    }
}
