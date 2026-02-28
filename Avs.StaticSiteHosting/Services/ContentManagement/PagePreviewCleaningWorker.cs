using System;
using System.Threading;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement;

public class PagePreviewCleaningWorker(
    MongoEntityRepository repository,
    ILogger<PagePreviewCleaningWorker> logger) : BackgroundService
{
    private readonly TimeSpan _cleaningInterval  = TimeSpan.FromMinutes(10);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var threshold = DateTime.UtcNow.AddDays(-1);
                var previews =
                    repository.GetEntityCollection<PagePreviewEntity>(GeneralConstants.PAGE_PREVIEW_COLLECTION);
                
                var deleteResult = await previews.DeleteManyAsync(x => x.Timestamp <= threshold, stoppingToken);
                if (deleteResult.DeletedCount > 0)
                {
                    logger.LogInformation("{deletedCount} page previews deleted", deleteResult.DeletedCount);
                }
                
                await Task.Delay(_cleaningInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Worker stopping at: {time}", DateTime.UtcNow);
            }
            catch (Exception e)
            {
               logger.LogError(e, "Error during page preview cleaning");
            }
        }
    }
}