using Avs.StaticSiteHosting.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Avs.StaticSiteHosting.DataMigrator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dbConnection = File.ReadAllText("db_connection.json");
            var mongoDbSettings = JsonConvert.DeserializeObject<MongoDbSettings>(dbConnection);

            var services = new ServiceCollection();
            services.Configure<MongoDbSettings>(opt => {
                opt.Database = mongoDbSettings.Database;
                opt.Host = mongoDbSettings.Host;
            });

            services.AddTransient<PasswordHasher>();
            services.AddSingleton<MongoEntityRepository>();

            var serviceProvider = services.BuildServiceProvider();
            var mongoEntityRepo = serviceProvider.GetRequiredService<MongoEntityRepository>();
            var passwordHasher = serviceProvider.GetRequiredService<PasswordHasher>();

            await DbInitialization.InitDbAsync(mongoEntityRepo, passwordHasher);
            await HelpContentInitializer.InitHelpData(mongoEntityRepo);

            if (serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Console.WriteLine("Migration has completed.");
        }
    }
}
