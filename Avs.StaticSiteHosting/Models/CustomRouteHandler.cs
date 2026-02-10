namespace Avs.StaticSiteHosting.Web.Models;

public class CustomRouteHandler : BaseEntity
{
    public string SiteId { get; set; }
    public string Path { get; set; }
    public string Method { get; set; }
    public string Body { get; set; }
}