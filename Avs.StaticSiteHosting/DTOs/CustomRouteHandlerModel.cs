namespace Avs.StaticSiteHosting.Web.DTOs;

public record CustomRouteHandlerModel(string Id, string SiteId, string Name, string Method, string Path, string Body);

public record CreateCustomRouteHandlerRequest(string SiteId, string Name, string Method, string Path, string Body);