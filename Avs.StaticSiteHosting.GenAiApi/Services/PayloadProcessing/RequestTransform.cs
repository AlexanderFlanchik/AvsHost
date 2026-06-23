using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public interface IRequestTransform
{
    ValueTask TransformAsync(byte[] requestBodyBuffer, HttpContext context, ModelSettings modelSettings);
}

public class RequestTransform(IJsonRemapper remapper) : IRequestTransform
{
    public async ValueTask TransformAsync(byte[] requestBodyBuffer, HttpContext context, ModelSettings modelSettings)
    {
        var mappingId = modelSettings.GetMappingId(PayloadMappingType.Request, context.Request.Path);
        if (string.IsNullOrEmpty(mappingId))
        {
            context.Request.Body = new MemoryStream(requestBodyBuffer);
            
            return; 
        }
        
        using var requestStream = new MemoryStream(requestBodyBuffer);
        
        Stream mappedStream = new MemoryStream();
        await remapper.TransformAsync(mappingId, requestStream, mappedStream);
        mappedStream.Position = 0;
        
        context.Request.Body = mappedStream;
    }
}