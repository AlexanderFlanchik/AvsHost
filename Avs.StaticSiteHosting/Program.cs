using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var staticSiteOptions = (IOptions<StaticSiteOptions>)host.Services.GetService(typeof(IOptions<StaticSiteOptions>));
            if (staticSiteOptions == null || 
                staticSiteOptions.Value == null || 
                string.IsNullOrEmpty(staticSiteOptions.Value.ContentPath))
            {
                throw new Exception("Invalid application configuration. Static site options were not found or configured.");
            }

            InitStorage(staticSiteOptions.Value.ContentPath);

            var mongoEntityRepository = (MongoEntityRepository)host.Services.GetService(typeof(MongoEntityRepository));
            if (mongoEntityRepository == null)
            {
                throw new Exception("Mongo DB data layer was not found.");
            }

            await DbInitialization.InitDbAsync(mongoEntityRepository);

            await host.RunAsync();
        }

        private static void InitStorage(string contentPath)
        {
            if (!Directory.Exists(contentPath))
            {
                Directory.CreateDirectory(contentPath);
                Console.WriteLine("Content folder created!");
            } else {
                Console.WriteLine($"Found static content storage: {contentPath}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}