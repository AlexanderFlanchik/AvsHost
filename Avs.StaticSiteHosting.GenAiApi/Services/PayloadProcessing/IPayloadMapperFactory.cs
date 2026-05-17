using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public interface IPayloadMapperFactory
{
    IPayloadMapper? GetPayloadMapper(string requestPath, PayloadMappingType mappingType);
}