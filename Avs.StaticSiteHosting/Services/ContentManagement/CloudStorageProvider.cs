using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Web.Common;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public interface ICloudStorageProvider
    {
        Task<Stream> GetCloudContent(string user, string siteName, string contentName);
        Task UploadContentToCloud(string user, string siteName, string contentName, Stream contentStream);
        Task DeleteContentFromCloud(string user, string siteName, string contentName);
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
            var getContentRequest = new GetObjectRequest()
            {
                BucketName = _cloudStorageSettings.BucketName,
                Key = GetContentKey(user, siteName, contentName)
            };

            GetObjectResponse getObjectResponse;

            try
            {
                using var s3Client = CreateS3Client();
                
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

        public async Task UploadContentToCloud(string user, string siteName, string contentName, Stream contentStream)
        {            
            var putObjectRequest = new PutObjectRequest()
            {
                BucketName = _cloudStorageSettings.BucketName,
                Key = GetContentKey(user, siteName, contentName),
                InputStream = contentStream,
                AutoCloseStream = true
            };

            try
            {
                using var client = CreateS3Client();

                var response = await client.PutObjectAsync(putObjectRequest);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("The file '{0}' successfully saved to cloud storage", contentName);
                }
                else
                {
                    _logger.LogWarning("Unable to save '{0}' to cloud storage. The service responded with code '{1}'",
                        contentName, (int)response.HttpStatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to save '{0}' to cloud storage because of exception", contentName);
            }
        }

        public async Task DeleteContentFromCloud(string user, string siteName, string contentName)
        {
            var deleteObjectRequest = new DeleteObjectRequest()
            {
                BucketName = _cloudStorageSettings.BucketName,
                Key = GetContentKey(user, siteName, contentName)
            };

            try
            {
                using var s3Client = CreateS3Client();
                _ = await s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to delete '{0}' from the cloud storage because of exception", contentName);
            }
        }

        private IAmazonS3 CreateS3Client() => new AmazonS3Client(_cloudStorageSettings.AccessKey,
                                                              _cloudStorageSettings.Secret,
                                                              RegionEndpoint.GetBySystemName(_cloudStorageSettings.Region));

        private string GetContentKey(string user, string siteName, string contentName) => $"{user}/{siteName}/{contentName}".Replace('\\', '/');
    }
}