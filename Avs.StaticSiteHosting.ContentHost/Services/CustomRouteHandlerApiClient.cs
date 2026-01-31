namespace Avs.StaticSiteHosting.ContentHost.Services;

public class CustomRouteHandlerApiClient(HttpClient client)
{
    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        return client.SendAsync(request);
    }
}