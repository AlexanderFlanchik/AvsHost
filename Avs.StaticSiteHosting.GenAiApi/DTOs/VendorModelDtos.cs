using System.ComponentModel.DataAnnotations;
using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Services;

namespace Avs.StaticSiteHosting.GenAiApi.DTOs;

#region Vendor

public record VendorResponse(
    string Id,
    string Name,
    string Code,
    string BaseUrl,
    string? Description,
    string? CustomSettings = null);

public record CreateVendorRequest(
    [Required] string Name, 
    [Required] string Code, 
    string Description,
    [Required] string BaseUrl,
    [Required] string ApiKey,
    string CustomSettings);

public record UpdateVendorRequest(
    string? Description,
    string? BaseUrl,
    string? ApiKey,
    string? CustomSettings
);

#endregion

#region Model

public record ModelData(
    [Required] string Name, 
    [Required] string VendorCode, 
    [Required] string[] Paths, 
    string[] Features,
    bool IsActive = true,
    string? DefaultModelUrl = null,
    PayloadMappingRef[]? PayloadMappings = null,
    string? CustomSettings = null);

public record UpdateModelRequest(string Id, ModelData Model);
public record GetModelQuery(int Page = 1, int PageSize = 50);
public record ModelResponse(
    string Id,
    string Name,
    string VendorId,
    string[] Paths, 
    string[] Features,
    string? CustomSettings = null);

#endregion