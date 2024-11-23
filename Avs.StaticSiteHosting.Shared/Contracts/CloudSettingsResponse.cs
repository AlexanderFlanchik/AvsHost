using Avs.StaticSiteHosting.Shared.Common;

namespace Avs.StaticSiteHosting.Shared.Contracts;

public class CloudSettingsResponse
{
    public CloudStorageSettings? StorageSettings { get; set; }
}