using System.Text;
using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public interface IResponseTransform
{
    Task TransformAsync(HttpResponse response, ModelSettings modelSettings, HttpResponseMessage llmResponse);
}

public class ResponseTransform(IPayloadMapperFactory mapperFactory, ILogger<ResponseTransform> logger) : IResponseTransform
{
    private const string _dataPrefix = "data:";
    private static readonly string[] _streamingTypes = ["text/event-stream"];
    
    public async Task TransformAsync(HttpResponse response, ModelSettings modelSettings, HttpResponseMessage llmResponse)
    {
        /*
         * 1. Check if there are mapping for response type, if not just copy llmResponse to response.Body
         * 2. If llmResponse is just raw one, map it using mapper and write to response.Body
         * 3. If it is a stream. map each chunk and write to response.Body
         */
        string requestPath = response.HttpContext.Request.Path;
        CancellationToken cancellationToken = response.HttpContext.RequestAborted;
        bool isStreaming = IsStreaming(llmResponse);
        PayloadMappingType mappingType = isStreaming
            ? PayloadMappingType.ResponseStream
            : PayloadMappingType.Response;
        
        var mapper = mapperFactory.GetPayloadMapper(requestPath, mappingType);
        await using var responseStream = await llmResponse.Content.ReadAsStreamAsync(cancellationToken);
        
        if (mapper is null)
        {
            await responseStream.CopyToAsync(response.Body, cancellationToken);

            return;
        }

        if (!isStreaming)
        {
            await ProcessRawResponseAsync(responseStream, response, mapper, modelSettings, cancellationToken);
        }
        else
        {
           await ProcessStreamResponseAsync(responseStream, response, mapper, modelSettings, cancellationToken);
        }
    }

    private async Task ProcessRawResponseAsync(
        Stream llmResponseStream, 
        HttpResponse response, 
        IPayloadMapper mapper, 
        ModelSettings modelSettings,
        CancellationToken cancellationToken)
    {
        if (!modelSettings.PayloadMappings.TryGetValue(PayloadMappingType.Response, out var mappingId)
            || string.IsNullOrEmpty(mappingId))
        {
            await llmResponseStream.CopyToAsync(response.Body, cancellationToken);

            return;
        }

        try
        {
            var mappedResponseStream = mapper.MapPayload(llmResponseStream, mappingId);
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
        IPayloadMapper mapper,
        ModelSettings modelSettings,
        CancellationToken cancellationToken)
    {
        if (!modelSettings.PayloadMappings.TryGetValue(PayloadMappingType.ResponseStream, out var mappingId)
            || string.IsNullOrEmpty(mappingId))
        {
            await llmResponseStream.CopyToAsync(response.Body, cancellationToken);

            return;
        }
        
        using var streamReader = new StreamReader(llmResponseStream, leaveOpen: true);

        while (await streamReader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith(_dataPrefix))
            {
                continue;
            }
            
            string chunk = line.Substring(_dataPrefix.Length);
            try
            {
                await using var mappedChunkStream = mapper.MapPayload(chunk, mappingId);
                await mappedChunkStream.CopyToAsync(response.Body, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to map LLM response fragment to OpenAI format");
                
                ReadOnlyMemory<byte> chunkBuffer = Encoding.UTF8.GetBytes($"{_dataPrefix}{chunk}");
                await response.Body.WriteAsync(chunkBuffer, cancellationToken);
            }
        }
    }

    private bool IsStreaming(HttpResponseMessage llmResponse)
    {
        return _streamingTypes.Contains(llmResponse.Content.Headers.ContentType?.MediaType ?? "application/json");
    }
}