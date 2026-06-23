namespace Avs.StaticSiteHosting.GenAiApi.Services;

public record VendorSettings(List<ModelSettings> Models)
{
    public string VendorId { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string ApiKey { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public string? CustomSettings { get; set; }
}