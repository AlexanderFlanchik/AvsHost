using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public class ModelSettings
{
    private IDictionary<string, string> _payloadMappings = new Dictionary<string, string>();

    public string ModelId { get; init; } = null!;
    public string VendorId { get; init; } = null!;
    public string VendorCode { get; init; } = null!;
    public string Name { get; init; } = null!; 
    public string? CustomSettings { get; set; }
    public string? DefaultModelUrl { get; set; }
    public string[] Paths { get; set; } = [];
    public string PublicName => $"{VendorCode}/{Name}";
    
    public string? GetMappingId(PayloadMappingType type, string requestPath)
    {
        return _payloadMappings.TryGetValue(GetMappingKey(type, requestPath), out var mappingId) ?
            mappingId : null;
    }
    
    public void SetPayloadMappings(IEnumerable<PayloadMappingRef> payloadMappings)
    {
        foreach (var payloadMapping in payloadMappings)
        {
            _payloadMappings[GetMappingKey(payloadMapping.Type, payloadMapping.RequestPath)] = payloadMapping.MappingId;
        }
    }

    private static string GetMappingKey(PayloadMappingType type, string requestPath) 
        => $"PayloadMapping_{type.ToString()}_{requestPath}";
}