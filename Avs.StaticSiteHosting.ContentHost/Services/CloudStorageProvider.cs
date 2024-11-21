using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Avs.StaticSiteHosting.Shared.Common;
using System.Net;

namespace Avs.StaticSiteHosting.ContentHost.Services
{
    public interface ICloudStorageProvider
    {
        Task<Stream?> GetCloudContent(string user, string siteName, string contentName);
        Task UploadContentToCloud(string user, string siteName, string contentName, Stream contentStream);
    }

    public class CloudStorageProvider(
        CloudStorageSettings cloudStorageSettings, 
        ILogger<CloudStorageProvider> logger) : ICloudStorageProvider
    {
        public async Task<Stream?> GetCloudContent(string user, string siteName, string contentName)
        {
            var getContentRequest = new GetObjectRequest()
            {
                BucketName = cloudStorageSettings.BucketName,
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
                logger.LogError(ex, "Unable to get content from S3 cloud.");

                return null;
            }

            var stream = getObjectResponse.ResponseStream;

            return stream;
        }

        public async Task UploadContentToCloud(string user, string siteName, string contentName, Stream contentStream)
        {
            var putObjectRequest = new PutObjectRequest()
            {
                BucketName = cloudStorageSettings.BucketName,
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
                    logger.LogInformation("The file '{0}' successfully saved to cloud storage", contentName);
                }
                else
                {
                    logger.LogWarning("Unable to save '{0}' to cloud storage. The service responded with code '{1}'",
                        contentName, (int)response.HttpStatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to save '{0}' to cloud storage because of exception", contentName);
            }
        }

        // TODO: Move it to shared helper class
        private IAmazonS3 CreateS3Client() => new AmazonS3Client(cloudStorageSettings.AccessKey,
                                                             cloudStorageSettings.Secret,
                                                             RegionEndpoint.GetBySystemName(cloudStorageSettings.Region));

        private string GetContentKey(string user, string siteName, string contentName) => $"{user}/{siteName}/{contentName}".Replace('\\', '/');
    }
}