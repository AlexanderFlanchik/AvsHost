using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avs.StaticSiteHosting
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            InitStorage();

            await host.RunAsync();
        }

        private static void InitStorage()
        {
            var contentPath = Path.Combine("Content");
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
