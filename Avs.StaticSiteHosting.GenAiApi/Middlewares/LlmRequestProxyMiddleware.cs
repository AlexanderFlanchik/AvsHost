using System.Text.Json;
using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Services;

namespace Avs.StaticSiteHosting.GenAiApi.Middlewares;

public class LlmRequestProxyMiddleware(ILlmProvider llmProvider, ILogger<LlmRequestProxyMiddleware> logger) : IMiddleware
{
    private readonly string[] _skipRoutes = ["api/vendor", "api/model", "api/payloadmapping"];
    
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var requestPath = context.Request.Path.Value?.Replace(AppConstants.ApiSegment, string.Empty);
        if (!HttpMethods.IsPost(context.Request.Method) ||
            _skipRoutes.Contains(requestPath, StringComparer.InvariantCultureIgnoreCase))
        {
            await next(context);
            return;
        }
        
        using var reader = new StreamReader(context.Request.Body);
        var requestBody = await reader.ReadToEndAsync(context.RequestAborted);
        try
        {
            using var requestPayload = JsonDocument.Parse(requestBody);
            var modelName = string.Empty;
            if (requestPayload.RootElement.TryGetProperty("model", out var modelElement) &&
                modelElement.ValueKind == JsonValueKind.String)
            {
                modelName = modelElement.GetString() ?? string.Empty;
            }

            ModelSettings? model;
            
            if (!string.IsNullOrEmpty(modelName))
            {
                model = modelName.Contains('/') ? llmProvider.GetAllModels().FirstOrDefault(m => m.PublicName == modelName) : 
                    llmProvider.GetAllModels().FirstOrDefault(m => m.Name == modelName);
                requestBody = modelName.Contains('/') && model is not null ? requestBody.Replace(modelName, model.Name) : requestBody;
            }
            else
            {
                model = llmProvider.GetAllModels().FirstOrDefault(m => m.Paths.Contains(requestPath));
            }
            
            if (model is null || !model.Paths.Contains(requestPath))
            {
                throw new InvalidVendorOrModelException($"Model '{modelName}' not found or does not have path '{requestPath}'");
            }

            var vendor = llmProvider.GetVendorById(model.VendorId);
            if (vendor is null)
            {
                throw new InvalidVendorOrModelException($"Vendor #{model.VendorId} not found");
            }

            var client = context.RequestServices.GetRequiredService<IProxyClient>();
            await client.ProxyLlmRequestAsync(vendor, model, requestBody, context);
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
}