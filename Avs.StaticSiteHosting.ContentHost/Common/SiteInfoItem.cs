using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Common;

public class SiteInfoItem
{
    public SiteContentInfo? SiteContentInfo { get; set; }
    public string? SitePath { get; set; }
    public string? ContentCacheKey { get; set; }
}