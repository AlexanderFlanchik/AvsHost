using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Common
{
    public class CloudStorageSettingsWorker : IHostedService
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger<CloudStorageSettingsWorker> _logger;
        private readonly CloudStorageSettings _settings;

        public CloudStorageSettingsWorker(ISettingsManager settingsManager, 
                ILogger<CloudStorageSettingsWorker> logger, 
                CloudStorageSettings settings)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            _settings = settings;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var settingsData = await _settingsManager.GetAsync(CloudStorageSettings.SettingsName);
            if (settingsData == null)
            {
                _logger.LogWarning("No AWS Cloud settings found. Please set up.");
                return;
            }

            var settings = JsonConvert.DeserializeObject<CloudStorageSettings>(settingsData.Value);
            _settings.CopyFrom(settings);

            _logger.LogInformation("Cloud storage configuration has been loaded.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping CloudStorageSettingsWorker service..");

            return Task.CompletedTask;
        }
    }
}