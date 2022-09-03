namespace Avs.StaticSiteHosting.Web.DTOs
{
    public record ContentInputContext(string ContentId, string ContentName, string SiteId, string UploadSessionId, string DestinationPath);
}