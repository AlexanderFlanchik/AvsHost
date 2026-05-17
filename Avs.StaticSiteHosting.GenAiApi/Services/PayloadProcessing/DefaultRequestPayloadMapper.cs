using System.Text;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public class DefaultRequestPayloadMapper(IPayloadMappingProvider mappingProvider) : IPayloadMapper
{
    public Stream MapPayload(Stream inputPayload, string mappingId)
    {
        var requestMappings = mappingProvider.GetMappings(mappingId);
        if (requestMappings.Length == 0)
        {
            return inputPayload;
        }
        
        return PayloadMapper.MapPayloadStream(inputPayload, requestMappings);
    }

    public Stream MapPayload(string inputJson, string mappingId)
    {
        var requestMappings = mappingProvider.GetMappings(mappingId);
        if (requestMappings.Length == 0)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(inputJson));
        }
        
        return PayloadMapper.MapPayloadStream(inputJson, requestMappings);
    }
}