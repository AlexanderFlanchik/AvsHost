using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Avs.StaticSiteHosting.DataMigrator;

public class AppInitializer(
    DbInitializer dbInitializer,
    HelpContentInitializer helpContentInitializer,
    ILogger<AppInitializer> logger,
    
    IHostApplicationLifetime lifetime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbInitializer.InitDbAsync();
            await helpContentInitializer.InitHelpData();
            
            logger.LogInformation("Data initialized successfully.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to initialize data");
        }
        
        lifetime.StopApplication();      
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}