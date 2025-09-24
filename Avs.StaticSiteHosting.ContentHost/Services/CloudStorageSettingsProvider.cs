using Avs.Messaging.Contracts;
using Avs.Messaging.RabbitMq;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Services
{
    public interface ICloudStorageSettingsProvider
    {
        Task<CloudStorageSettings?> GetCloudStorageSettingsAsync();
    }

    public class CloudStorageSettingsProvider(IServiceProvider serviceProvider) : ICloudStorageSettingsProvider
    {
        public async Task<CloudStorageSettings?> GetCloudStorageSettingsAsync()
        {
            using var scope = serviceProvider.CreateScope();
            var requestClient = scope.ServiceProvider.GetRequiredKeyedService<IRpcClient>(RabbitMqOptions.TransportName); //<IRequestClient<CloudSettingsRequest>>();
            var settingsResponse =
                await requestClient.RequestAsync<CloudSettingsRequest, CloudSettingsResponse>(
                    new CloudSettingsRequest());
            
            return settingsResponse.StorageSettings;
        }
    }
}