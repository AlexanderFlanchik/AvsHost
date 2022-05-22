using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Common
{
    public class CloudStorageSyncWorker : BackgroundService
    {
        private ConcurrentQueue<SyncContentTask> _tasks = new ConcurrentQueue<SyncContentTask>();
        private readonly ICloudStorageProvider _cloudStorageProvider;
        private readonly ILogger<CloudStorageSyncWorker> _logger;

        public CloudStorageSyncWorker(ICloudStorageProvider cloudStorageProvider, ILogger<CloudStorageSyncWorker> logger)
        {
            _cloudStorageProvider = cloudStorageProvider;
            _logger = logger;
        }

        public void Add(SyncContentTask task) => _tasks.Enqueue(task);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                while (_tasks.TryDequeue(out SyncContentTask task))
                {
                    _logger.LogInformation("Start downloading {0} from cloud bucket", task.ContentName);

                    var fi = new FileInfo(task.FullFileName);
                    using var contentStream = await _cloudStorageProvider.GetCloudContent(task.UserName, task.SiteName, task.ContentName);

                    using var fileStream = fi.OpenWrite();
                    await contentStream.CopyToAsync(fileStream);

                    _logger.LogInformation("The content '{0}' has been successfully saved locally.", task.FullFileName);
                }

                await Task.Delay(5000);
            }
        }
    }

    public class SyncContentTask
    {
        /// <summary>
        /// Content relative name with destination path (optional)
        /// </summary>
        public string ContentName { get; set; }

        /// <summary>
        /// Content physical file full name
        /// </summary>
        public string FullFileName { get; set; }

        /// <summary>
        /// Site user name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName { get; set; }
    }
}