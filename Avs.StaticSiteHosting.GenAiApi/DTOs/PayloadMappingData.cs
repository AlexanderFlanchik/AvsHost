using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.DTOs;

public class PayloadMappingData
{
    public string JsonContent { get; set; } = null!;
    public string VendorCode { get; init; } = null!;
    public string Model { get; init; } = null!;
    public PayloadMappingType PayloadMappingType { get; init; }
}