using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public class JsonRemappingContext
{
    public JsonElement Root { get; init; }
    public required IDictionary<string, string> Items { get; init; }
}