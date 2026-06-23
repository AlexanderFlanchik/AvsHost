using System.Text.Json;
using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Services;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

namespace Avs.StaticSiteHosting.GenAiApi.Middlewares;

public class LlmRequestProxyMiddleware(
    ILlmProvider llmProvider, 
    IRequestTransform requestTransform, 
    ILogger<LlmRequestProxyMiddleware> logger) : IMiddleware
{
    private readonly string[] _skipRoutes = ["api/vendor", "api/model", "api/payloadmapping"];
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestPath = (context.Request.Path.Value?[1..]!).Replace(AppConstants.ApiSegment, string.Empty);
        if (!HttpMethods.IsPost(context.Request.Method) ||
            _skipRoutes.Any(s => requestPath.StartsWith(s, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        byte[] requestBodyBuffer = await GetRequestBodyBuffer(context.Request);
        try
        {
            using var requestPayload = JsonDocument.Parse(requestBodyBuffer);
            var modelName = string.Empty;
            if (requestPayload.RootElement.TryGetProperty("model", out var modelElement) &&
                modelElement.ValueKind == JsonValueKind.String)
            {
                modelName = modelElement.GetString() ?? string.Empty;
            }

            ModelSettings? model;

            if (!string.IsNullOrEmpty(modelName))
            {
                model = modelName.Contains('/')
                    ? llmProvider.GetAllModels().FirstOrDefault(m => m.PublicName == modelName)
                    : llmProvider.GetAllModels().FirstOrDefault(m => m.Name == modelName);
                
                using var updatedBody = GetUpdatedBody(requestPayload, model?.Name);
                requestBodyBuffer = updatedBody.ToStream().ToArray();
            }
            else
            {
                model = llmProvider.GetAllModels().FirstOrDefault(m => m.Paths.Contains(requestPath));
            }

            if (model is null || !model.Paths.Contains(requestPath))
            {
                throw new InvalidVendorOrModelException(
                    $"Model '{modelName}' not found or does not have path '{requestPath}'");
            }

            var vendor = llmProvider.GetVendorById(model.VendorId);
            if (vendor is null)
            {
                throw new InvalidVendorOrModelException($"Vendor #{model.VendorId} not found");
            }
            
            await requestTransform.TransformAsync(requestBodyBuffer, context, model);
            var client = context.RequestServices.GetRequiredService<IProxyClient>();
            await client.ProxyLlmRequestAsync(context, vendor, model);
        }
        catch (JsonException)
        {
            await next(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while processing request");
            await context.Response.WriteAsync(
                e is InvalidVendorOrModelException ? e.Message :
                "Unable to process request because of server error", context.RequestAborted);
            
            context.Response.StatusCode = e is InvalidVendorOrModelException ?
                StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        }
    }

    private static async Task<byte[]> GetRequestBodyBuffer(HttpRequest request)
    {
        using var stream = new MemoryStream();
        await request.Body.CopyToAsync(stream);
        stream.Position = 0;
        
        return stream.ToArray();
    }
    
    private JsonDocument GetUpdatedBody(JsonDocument requestPayload, string? model)
    {
        if (string.IsNullOrEmpty(model))
        {
            return requestPayload;
        }
        
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented =  true });
        
        writer.WriteStartObject();
        foreach (var element in requestPayload.RootElement.EnumerateObject())
        {
            if (element.Name.Equals("model", StringComparison.OrdinalIgnoreCase))
            {
                writer.WriteString("model", model);
            }
            else
            {
                element.WriteTo(writer);
            }
        }
        
        writer.WriteEndObject();
        writer.Flush();
        
        stream.Position = 0;
        
        return JsonDocument.Parse(stream);
    }
}