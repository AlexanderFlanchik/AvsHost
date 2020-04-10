using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Avs.StaticSiteHosting
{
    public static class ApplicationBuilderExtentions
    {
        public static IApplicationBuilder UseDashboard(this IApplicationBuilder app)
        {
            var distPath = Path.Combine(new DirectoryInfo("ClientApp").FullName, "dist");
            app.UseFileServer(new FileServerOptions { FileProvider = new PhysicalFileProvider(distPath) });

            return app;
        }
    }
}