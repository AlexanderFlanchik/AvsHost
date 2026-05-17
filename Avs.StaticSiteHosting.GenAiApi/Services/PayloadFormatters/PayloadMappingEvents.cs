using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public record MappingPayloadChanged(string MappingId, string VendorCode, string JsonContent, PayloadMappingType Type); 
public record MappingDeleted(string MappingId);