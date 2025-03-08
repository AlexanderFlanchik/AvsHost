using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.Shared.Common;
using System.Collections.Concurrent;

namespace Avs.StaticSiteHosting.ContentHost.Services
{
    public class CloudStorageWorker(
        CloudStorageSettings storageSettings, 
        ICloudStorageSettingsProvider settingsProvider,
        ICloudStorageProvider cloudStorageProvider,
        ILogger<CloudStorageWorker> logger) : BackgroundService
    {
        private bool _isInitialized = false;
        private ConcurrentQueue<SyncContentTask> _tasks = new ConcurrentQueue<SyncContentTask>();

        public void Add(SyncContentTask task) => _tasks.Enqueue(task);
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(30_000, stoppingToken);
                var settings = await settingsProvider.GetCloudStorageSettingsAsync();
                if (settings is null)
                {
                    logger.LogWarning("No cloud storage settings received. Cloud storage functionality is disabled.");

                    return;
                }

                storageSettings.CopyFrom(settings);
                            
                _isInitialized = true;

                logger.LogInformation("The cloud storage feature has been initialized successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to initialize cloud storage. Cloud storage functionality is disabled.");
            }

            if (!_isInitialized)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                while (_tasks.TryDequeue(out var task))
                {
                    logger.LogInformation("Start downloading {0} from cloud bucket", task.ContentName);

                    var fi = new FileInfo(task.FullFileName);
                    using var contentStream = await cloudStorageProvider.GetCloudContent(task.UserName, task.SiteName, task.ContentName);
                    if (contentStream is null)
                    {
                        logger.LogWarning("Unable to load content '{contentName}' for site '{siteName}'", task.ContentName, task.SiteName);
                        
                        continue;
                    }

                    using var fileStream = fi.OpenWrite();
                    await contentStream.CopyToAsync(fileStream);

                    logger.LogInformation("The content '{0}' has been successfully saved locally.", task.FullFileName);
                }

                await Task.Delay(5000);
            }
        }
    }
}