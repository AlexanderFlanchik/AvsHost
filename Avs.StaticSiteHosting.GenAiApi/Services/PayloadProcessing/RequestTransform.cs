using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public interface IRequestTransform
{
    ValueTask TransformAsync(HttpRequest request, ModelSettings modelSettings);
}

public class RequestTransform(IPayloadMapperFactory mapperFactory) : IRequestTransform
{
    public async ValueTask TransformAsync(HttpRequest request, ModelSettings modelSettings)
    {
        if (!modelSettings.PayloadMappings.TryGetValue(PayloadMappingType.Request, out var mappingId) 
            || string.IsNullOrEmpty(mappingId))
        {
            return; 
        }

        var payloadMapper = mapperFactory.GetPayloadMapper(request.Path, PayloadMappingType.Request);
        if (payloadMapper is null)
        {
            return;
        }
        
        Stream mappedStream;
        await using (request.Body)
        {
            mappedStream = payloadMapper.MapPayload(request.Body, mappingId!);
        }
        
        request.Body = mappedStream;
    }
}