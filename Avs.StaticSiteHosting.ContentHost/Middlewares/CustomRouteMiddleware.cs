using System.Text;
using System.Text.Json;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Contracts;
using Avs.StaticSiteHosting.ContentHost.Services;
using Avs.StaticSiteHosting.Shared.Contracts;
using Microsoft.Extensions.Primitives;

namespace Avs.StaticSiteHosting.ContentHost.Middlewares;

public class CustomRouteMiddleware(IServiceProvider serviceProvider, ILogger<CustomRouteMiddleware> logger)
    : IMiddleware
{
    private static readonly JsonSerializerOptions _jsonSettings = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    
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

        var apiClient = serviceProvider.GetRequiredService<CustomRouteHandlerApiClient>();
        
        try
        {
            using var request =
                await PrepareRequest(routeMethod, siteItem.SiteContentInfo.DatabaseName, routeMatched, context);
            using var routeResponse = await apiClient.SendAsync(request);
            context.Response.StatusCode = (int)routeResponse.StatusCode;
            
            SetResponseHeaders(routeResponse, context.Response);
            
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

    private async Task<HttpRequestMessage> PrepareRequest(string routeMethod, string? dbName, CustomRouteInfo routeMatched, HttpContext context)
    {
        var request = new HttpRequestMessage();
        string url = "handle";
        string? body = null;

        if (routeMethod == HttpMethods.Post || routeMethod == HttpMethods.Put)
        {
            using var streamReader = new StreamReader(context.Request.Body);
            body = await streamReader.ReadToEndAsync();
        }

        request.RequestUri = new Uri(url, UriKind.Relative);
        request.Method = HttpMethod.Post;
        
        var routeRequest = new RouteHandlerRequest()
        {
            HandlerId = routeMatched.HandlerId,
            DbName = dbName,
            Body = body
        };
        
        FillRequestHeadersAndQuery(routeRequest, context);
        
        string requestJson = JsonSerializer.Serialize(routeRequest, _jsonSettings);
        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        return request;
    }

    private void FillRequestHeadersAndQuery(RouteHandlerRequest routeRequest, HttpContext context)
    {
        routeRequest.Headers = GetData(context.Request.Headers);
        routeRequest.Query = GetData(context.Request.Query);
        
        return;

        static IDictionary<string, string>? GetData(IEnumerable<KeyValuePair<string, StringValues>> source)
        {
            using var enumerator = source.GetEnumerator();
            var dictionary = new Dictionary<string, string>();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                dictionary.Add(current.Key, current.Value!);
            }
            
            return dictionary.Count > 0 ? dictionary : null;
        }
    }

    private void SetResponseHeaders(HttpResponseMessage apiResponse, HttpResponse response)
    {
        var headers = apiResponse.Headers.ToList();
        headers.AddRange(apiResponse.Content.Headers);
        
        foreach (var contentHeader in headers)
        {
            response.Headers[contentHeader.Key] = string.Join(",", contentHeader.Value!);    
        }
    }
}