using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public class DefaultProxyClient(HttpClient client, ILogger<DefaultProxyClient> logger) : IProxyClient
{
    private static readonly string[] _contentHeaders = ["Content-Type", "Content-Length"];
    private static readonly string[] _streamingTypes = ["application/x-ndjson", "text/event-stream"];
    
    public async Task ProxyLlmRequestAsync(VendorSettings vendor, ModelSettings model, string requestBodyContent, HttpContext context)
    {
        var proxyUrl = GetProxyUrl(vendor, model, context.Request.Path);
        var cancellationToken = context.RequestAborted;
        
        var content = new StringContent(requestBodyContent, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, proxyUrl)
        {
            Content = content
        };
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", vendor.ApiKey);
        
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        
        context.Response.StatusCode = (int)response.StatusCode;        
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError(
                    "LLM request failed. Model: {vendorCode}/{model}, Error: {errorResponse}", 
                    vendor.Code, model.Name, errorResponse);
            
            await context.Response.WriteAsync(errorResponse, cancellationToken);
            
            return;
        }
        
        CopyHeaders(response, context.Response.Headers);
        // await responseTransform.TransformAsync(context.Response, model, response);
        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await responseStream.CopyToAsync(context.Response.Body, cancellationToken);
    }

    private string GetProxyUrl(VendorSettings vendor, ModelSettings model, string requestPath)
    {
        var modelPath = requestPath.Replace(AppConstants.ApiSegment, string.Empty);
        if (string.IsNullOrEmpty(model.CustomSettings))
        {
            return $"{vendor.BaseUrl}/{modelPath}";
        }
        
        JsonObject? settingsObj = null; 
        try
        {
            settingsObj = JsonNode.Parse(model.CustomSettings)!.AsObject();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error parsing model custom settings for {vendorCode}/{modelName}", vendor.Code, model.Name);
        }
            
        if (settingsObj!.TryGetPropertyValue("modelUrl", out var modelUrlObj) && modelUrlObj is not null)
        {
            return modelUrlObj.GetValue<string>();
        }

        return $"{vendor.BaseUrl}/{modelPath}";
    }

    private static void CopyHeaders(HttpResponseMessage response, IHeaderDictionary headers)
    {
        foreach (var header in response.Headers)
        {
            if (headers.ContainsKey(header.Key))
            {
                continue;
            }

            headers[header.Key] = header.Value.ToArray();
        }
        
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";
        headers.ContentType = contentType;
        
        if (!_streamingTypes.Contains(contentType))
        {
            headers.ContentLength = response.Content.Headers.ContentLength;
        }
        
        foreach (var header in response.Content.Headers)
        {
            if (_contentHeaders.Contains(header.Key))
            {
                continue;
            }
            
            headers[header.Key] = header.Value.ToArray();
        }
    }
}