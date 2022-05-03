namespace Avs.StaticSiteHosting.Web.Common
{
    /// <summary>
    /// Represents general settings for AWS S3 content storage access.
    /// </summary>
    public class CloudStorageSettings
    {
        public const string SettingsName = nameof(CloudStorageSettings);
        public bool Enabled { get; set; }
        public string BucketName { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string Secret { get; set; }
    }
}