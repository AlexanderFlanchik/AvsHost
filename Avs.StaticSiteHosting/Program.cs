using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting.Web
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
            InitStorage(staticSiteOptions.Value.TempContentPath);
          
            await host.RunAsync();
        }

        private static void InitStorage(string contentPath)
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}