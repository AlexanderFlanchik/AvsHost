namespace Avs.StaticSiteHosting.ContentHost.Services;

public class CustomRouteHandlerApiClient(HttpClient client)
{
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}