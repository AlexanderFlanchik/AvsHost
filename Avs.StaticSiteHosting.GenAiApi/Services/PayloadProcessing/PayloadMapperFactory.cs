using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadProcessing;

public class PayloadMapperFactory(IServiceProvider serviceProvider) : IPayloadMapperFactory
{
    public IPayloadMapper? GetPayloadMapper(string requestPath, PayloadMappingType mappingType)
    {
        var key = $"payloadMapper:{mappingType}:{requestPath}";
        return serviceProvider.GetKeyedService<IPayloadMapper>(key);
    }
}