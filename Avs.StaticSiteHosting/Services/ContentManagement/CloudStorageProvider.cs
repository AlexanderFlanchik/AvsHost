using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public interface ICloudStorageProvider
    {
        Task<Stream> GetCloudContent(string user, string siteName, string contentName);
    }

    public class CloudStorageProvider : ICloudStorageProvider
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ILogger<CloudStorageProvider> _logger;
        
        public CloudStorageProvider(ISettingsManager settingsManager, ILogger<CloudStorageProvider> logger)
        {
            _settingsManager = settingsManager;
            _logger = logger;
        }

        public async Task<Stream> GetCloudContent(string user, string siteName, string contentName)
        {
            var cloudSettingsEntry = await _settingsManager.GetAsync(CloudStorageSettings.SettingsName);
            if (cloudSettingsEntry == null)
            {
                throw new InvalidOperationException("Cloud settings is not configured.");
            }

            var cloudSettings = JsonConvert.DeserializeObject<CloudStorageSettings>(cloudSettingsEntry.Value);
            using var s3Client = new AmazonS3Client(cloudSettings.AccessKey, cloudSettings.Secret, RegionEndpoint.GetBySystemName(cloudSettings.Region));
            var getContentRequest = new GetObjectRequest()
            {
                BucketName = cloudSettings.BucketName,
                Key = $"{user}/{siteName}/{contentName}"
            };

            GetObjectResponse getObjectResponse;

            try
            {
                getObjectResponse = await s3Client.GetObjectAsync(getContentRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to get content from S3 cloud.");

                return null;
            }

            var stream = getObjectResponse.ResponseStream;

            return stream;
        }
    }
}