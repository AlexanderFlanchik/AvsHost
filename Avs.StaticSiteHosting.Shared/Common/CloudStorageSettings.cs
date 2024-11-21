namespace Avs.StaticSiteHosting.Shared.Common;

/// <summary>
/// Represents general settings for AWS S3 content storage access.
/// </summary>
public class CloudStorageSettings
{
    public const string SettingsName = nameof(CloudStorageSettings);

    public bool Enabled { get; set; }
    public string BucketName { get; set; } = default!;
    public string Region { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string Secret { get; set; } = default!;

    public void CopyFrom(CloudStorageSettings source)
    {
        Enabled = source.Enabled;
        BucketName = source.BucketName;
        Region = source.Region;
        AccessKey = source.AccessKey;
        Secret = source.Secret;
    }
}