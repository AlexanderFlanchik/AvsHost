using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public class DefaultProxyClient(HttpClient client, IResponseTransform responseTransform, ILogger<DefaultProxyClient> logger)
    : IProxyClient
{
    private static readonly string[] _contentHeaders = ["Content-Type", "Content-Length"];
    private static readonly string[] _streamingTypes = ["text/event-stream", "application/x-ndjson"];
    
    public async Task ProxyLlmRequestAsync(HttpContext context, VendorSettings vendor, ModelSettings model)
    {
        var proxyUrl = GetProxyUrl(vendor, model, context.Request.Path);
        var cancellationToken = context.RequestAborted;
        var content = new StreamContent(context.Request.Body);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Post, proxyUrl) { Content = content };
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
        await responseTransform.TransformAsync(context.Response, model, response);
    }

    private string GetProxyUrl(VendorSettings vendor, ModelSettings model, string requestPath)
    {
        if (!string.IsNullOrEmpty(model.DefaultModelUrl))
        {
            return model.DefaultModelUrl.Replace("{apiKey}", vendor.ApiKey);
        }
        
        var modelPath = requestPath[1..].Replace(AppConstants.ApiSegment, string.Empty);
        
        return $"{vendor.BaseUrl}/{modelPath}";
    }

    private static void CopyHeaders(HttpResponseMessage response, IHeaderDictionary headers)
    {
        foreach (var header in response.Headers)
        {
            if (headers.ContainsKey(header.Key) || response.Headers.Contains("Transfer-Encoding"))
            {
                continue;
            }

            headers[header.Key] = header.Value.ToArray();
        }
        
        var contentType =  response.Content.Headers.ContentType?.ToString() ?? AppConstants.DefaultJsonType;
        headers.ContentType = contentType;

        if (!_streamingTypes.Contains(contentType) && !response.Headers.Contains("Transfer-Encoding"))
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