namespace Avs.StaticSiteHosting.ContentHost.Contracts;

public class RouteHandlerRequest
{
    public string HandlerId { get; set; } = default!;
    public string? DbName { get; set; }
    public IDictionary<string, string>? Headers { get; set; }
    public string? Body { get; set; }
    public IDictionary<string, string>? Query { get; set; }
}