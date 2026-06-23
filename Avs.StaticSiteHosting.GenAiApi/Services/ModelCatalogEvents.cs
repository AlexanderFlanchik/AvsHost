using Avs.StaticSiteHosting.GenAiApi.Common;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public record ModelCreated(string ModelId, string VendorId, string Name, string[] Paths, string? DefaultModelUrl, string? CustomSettings, PayloadMappingRef[] PayloadMappings);
public record ModelUpdated(string ModelId, string[] Paths, string? DefaultModelUrl, string? CustomSettings, PayloadMappingRef[] PayloadMappings);
public record ModelDeleted(string ModelId);
public record VendorUpdated(string VendorId, string BaseUrl, string ApiKey, string? CustomSettings);
public record VendorCreated(string VendorId, string VendorCode, string BaseUrl, string ApiKey, string? CustomSettings);
