namespace Avs.StaticSiteHosting.ContentHost.Common
{
    /// <summary>
    /// Represents a content download task from AWS S3
    /// </summary>
    /// <param name="ContentName">Name of content</param>
    /// <param name="FullFileName">Full file name</param>
    /// <param name="UserName">User name</param>
    /// <param name="SiteName">Name of site</param>
    public record SyncContentTask(string ContentName, string FullFileName, string UserName, string SiteName);
}