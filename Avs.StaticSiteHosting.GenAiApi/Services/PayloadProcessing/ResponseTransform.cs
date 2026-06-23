using System.Buffers;
using System.Text;
using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public interface IResponseTransform
{
    /// <summary>
    /// Transforms a response payload if needed
    /// </summary>
    /// <param name="response">HTTP response instance</param>
    /// <param name="modelSettings">Current LLM settings</param>
    /// <param name="llmResponse">LLM response</param>
    /// <returns>A task which completes when an HTTP response is handled</returns>
    Task TransformAsync(HttpResponse response, ModelSettings modelSettings, HttpResponseMessage llmResponse);
}

public class ResponseTransform(
    IJsonRemapper remapper,
    ILogger<ResponseTransform> logger) : IResponseTransform
{
    private static readonly string[] _streamingTypes = ["text/event-stream", "application/x-ndjson"];

    private delegate Task ResponseProcessor(
        Stream llmResponseStream, 
        HttpResponse response, 
        ModelSettings modelSettings, 
        CancellationToken cancellationToken);
    
    public async Task TransformAsync(HttpResponse response, ModelSettings modelSettings, HttpResponseMessage llmResponse)
    {
        CancellationToken cancellationToken = response.HttpContext.RequestAborted;
        bool isStreaming = IsStreaming(llmResponse);
        
        await using var responseStream = await llmResponse.Content.ReadAsStreamAsync(cancellationToken);
        ResponseProcessor processor = isStreaming ?
            ProcessStreamResponseAsync :
            ProcessRawResponseAsync;
        
        await processor(responseStream, response, modelSettings, cancellationToken);
    }

    private async Task ProcessRawResponseAsync(
        Stream llmResponseStream, 
        HttpResponse response, 
        ModelSettings modelSettings,
        CancellationToken cancellationToken)
    {
        var mappingId = modelSettings.GetMappingId(PayloadMappingType.Response, response.HttpContext.Request.Path);
        if (string.IsNullOrEmpty(mappingId))
        {
            await llmResponseStream.CopyToAsync(response.Body, cancellationToken);

            return;
        }

        try
        {
            using var mappedResponseStream = new MemoryStream();
            remapper.Transform(mappingId, llmResponseStream, mappedResponseStream);
            mappedResponseStream.Position = 0;
            
            await mappedResponseStream.CopyToAsync(response.Body, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to map LLM response to OpenAI format");
            await llmResponseStream.CopyToAsync(response.Body, cancellationToken);
        }
    }

    private async Task ProcessStreamResponseAsync(
        Stream llmResponseStream,
        HttpResponse response,
        ModelSettings modelSettings,
        CancellationToken cancellationToken)
    {
        var mappingId = modelSettings.GetMappingId(PayloadMappingType.ResponseStream, response.HttpContext.Request.Path);
        if (string.IsNullOrEmpty(mappingId))
        {
            await llmResponseStream.CopyToAsync(response.Body, cancellationToken);

            return;
        }
        
        using var streamReader = new StreamReader(llmResponseStream, leaveOpen: true);
        var writer = new ArrayBufferWriter<byte>();
        
        while (await streamReader.ReadLineAsync(cancellationToken) is { } line)
        {
            ReadOnlyMemory<byte> chunkBuffer = Encoding.UTF8.GetBytes(line);
            
            try
            {
                if (response.ContentType == "text/event-stream")
                {
                    remapper.TransformSse(mappingId, chunkBuffer, writer);
                }
                else
                {
                    remapper.TransformNdjson(mappingId, chunkBuffer, writer);
                }

                await response.Body.WriteAsync(writer.WrittenMemory, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to map LLM response fragment to OpenAI format");
                
                await response.Body.WriteAsync(chunkBuffer, cancellationToken);
            }
            finally
            {
                writer.Clear();
            }
        }
    }

    private static bool IsStreaming(HttpResponseMessage llmResponse)
    {
        var contentType = llmResponse.Content.Headers.ContentType?.MediaType ?? AppConstants.DefaultJsonType;
        if (llmResponse.Content.Headers.ContentLength is null && contentType == AppConstants.DefaultJsonType)
        {
            return true;
        }
        
        return _streamingTypes.Contains(contentType);
    }
}