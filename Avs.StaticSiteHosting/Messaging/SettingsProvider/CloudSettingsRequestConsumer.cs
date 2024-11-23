using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.Services.Settings;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Messaging.SettingsProvider
{
    public class CloudSettingsRequestConsumer(
        ISettingsManager settingsManager, 
        ILogger<CloudSettingsRequestConsumer> logger) : IConsumer<CloudSettingsRequest>
    {
        public async Task Consume(ConsumeContext<CloudSettingsRequest> context)
        {
            var settingsData = await settingsManager.GetAsync(CloudStorageSettings.SettingsName);
            if (settingsData == null)
            {
                logger.LogWarning("No AWS Cloud settings found. Please set up.");
                await context.RespondAsync(new CloudSettingsResponse());
                
                return;
            }

            var settings = JsonConvert.DeserializeObject<CloudStorageSettings>(settingsData.Value);
            var response = new CloudSettingsResponse()
            {
                StorageSettings = settings
            };
            
            await context.RespondAsync(response);
        }
    }
}