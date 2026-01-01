using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Services;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares;

public class CustomRouteMiddleware(IServiceProvider serviceProvider, ILogger<CustomRouteMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Items[Constants.SiteInfo] is not SiteInfoItem siteItem)
        {
            await next(context);
            return;
        }
        
        var routePath = context.Request.Path.Value ?? "/";
        var routeMethod = context.Request.Method;
        var routeMatched = siteItem.SiteContentInfo!.CustomRoutes.FirstOrDefault(r => 
            r.Method == routeMethod && r.Path == routePath.Replace($"/{siteItem.SiteContentInfo.Name}", string.Empty));
        
        if (routeMatched is null)
        {
            await next(context);
            return;
        }
        
        var queryString = context.Request.QueryString.Value;
        var apiClient = serviceProvider.GetRequiredService<CustomRouteHandlerApiClient>();
        
        using var request = new HttpRequestMessage();
        request.Headers.TryAddWithoutValidation(Constants.CustomRouteId, routeMatched.HandlerId);
        string url = "api/customer";
        
        if (!string.IsNullOrEmpty(queryString))
        {
            url += $"?{queryString}";
        }

        if (routeMethod == HttpMethods.Post || routeMethod == HttpMethods.Put)
        {
            using var ms = new MemoryStream();
            await context.Request.Body.CopyToAsync(ms);
            ms.Position = 0;
            request.Content = new StreamContent(ms);
        }
        
        request.RequestUri = new Uri(url);
        request.Method = HttpMethod.Post;

        try
        {
            using var routeResponse = await apiClient.SendAsync(request);
            context.Response.StatusCode = (int)routeResponse.StatusCode;
            
            await routeResponse.Content.CopyToAsync(context.Response.Body);
            await context.Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "Unable to process custom route {routeId}, path: {path}, HTTP method {method}", 
                routeMatched.HandlerId, 
                routePath, 
                routeMethod);
            throw;
        }
    }
}