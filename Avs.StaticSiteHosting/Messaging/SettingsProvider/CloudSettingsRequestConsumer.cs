using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Avs.Messaging.Contracts;
using Avs.Messaging.Core;

namespace Avs.StaticSiteHosting.Web.Messaging.SettingsProvider
{
    public class CloudSettingsRequestConsumer(
        ISettingsManager settingsManager, 
        ILogger<CloudSettingsRequestConsumer> logger) : ConsumerBase<CloudSettingsRequest>
    {
        protected override async Task Consume(MessageContext<CloudSettingsRequest> context)
        {
            var settingsData = await settingsManager.GetAsync(CloudStorageSettings.SettingsName);
            if (settingsData is null)
            {
                logger.LogWarning("No AWS Cloud settings found. Please set up.");
                await RespondAsync(new CloudSettingsResponse(), context);
                
                return;
            }

            var settings = JsonConvert.DeserializeObject<CloudStorageSettings>(settingsData.Value);
            var response = new CloudSettingsResponse()
            {
                StorageSettings = settings
            };
            
            await RespondAsync(response, context);
        }
    }
}