using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Avs.StaticSiteHosting.Web.Common;
using Microsoft.Extensions.Logging;
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
        private readonly CloudStorageSettings _cloudStorageSettings;
        private readonly ILogger<CloudStorageProvider> _logger;
        
        public CloudStorageProvider(CloudStorageSettings cloudStorageSettings, ILogger<CloudStorageProvider> logger)
        {
            _cloudStorageSettings = cloudStorageSettings ?? throw new ArgumentNullException(nameof(cloudStorageSettings));
            _logger = logger;
        }

        public async Task<Stream> GetCloudContent(string user, string siteName, string contentName)
        {
            using var s3Client = new AmazonS3Client(
                                        _cloudStorageSettings.AccessKey, 
                                        _cloudStorageSettings.Secret, 
                                        RegionEndpoint.GetBySystemName(_cloudStorageSettings.Region)
                                 );

            var getContentRequest = new GetObjectRequest()
            {
                BucketName = _cloudStorageSettings.BucketName,
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