using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting.Web.Common;

public class StorageInitializer(IOptions<StaticSiteOptions> staticSiteOptions, ILogger<StorageInitializer> logger) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Initializing content storage..");
        if (staticSiteOptions is null || staticSiteOptions.Value is null)
        {
            throw new InvalidOperationException("Invalid application configuration. Static site options were not found or configured.");
        }
        
        string contentPath = staticSiteOptions.Value.ContentPath, tempContentPath = staticSiteOptions.Value.TempContentPath;
        
        if (!string.IsNullOrWhiteSpace(contentPath))
        {
            InitStorage(contentPath);
        }
        else
        {
            logger.LogError("Content path is empty.");
            
            throw new InvalidOperationException();
        }

        if (!string.IsNullOrWhiteSpace(tempContentPath))
        {
            InitStorage(staticSiteOptions.Value.TempContentPath);
        }
        else
        {
            logger.LogError("Temp content path is empty.");
            throw new InvalidOperationException();
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
    
    
    private void InitStorage(string contentPath)
    {
        if (!Directory.Exists(contentPath))
        {
            Directory.CreateDirectory(contentPath!);
            logger.LogInformation("{contentPath} has been created.", contentPath);
        }
        else
        {
            logger.LogInformation("{contentPath} found.", contentPath);
        }
    }
}