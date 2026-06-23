namespace Avs.StaticSiteHosting.GenAiApi.Common;

public class PayloadMappingRef
{
    public string MappingId { get; set; } = null!;
    public PayloadMappingType Type { get; set; }
    public string RequestPath { get; set; } = null!;
}