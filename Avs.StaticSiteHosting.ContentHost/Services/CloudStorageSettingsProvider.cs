using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;
using MassTransit;

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
            var requestClient = scope.ServiceProvider.GetRequiredService<IRequestClient<CloudSettingsRequest>>();
            var settingsResponse = await requestClient.GetResponse<CloudSettingsResponse>(new CloudSettingsRequest());
            
            return settingsResponse.Message.StorageSettings;
        }
    }
}