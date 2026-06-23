using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// String-to-string remapping with pre-encoded output bytes.
/// Used by <c>$valueMap</c> directives to translate enum-ish values (e.g. role names).
/// </summary>
internal sealed class ValueMap
{
    private readonly Dictionary<string, JsonEncodedText> _map;

    public ValueMap(Dictionary<string, JsonEncodedText> map) => _map = map;

    public bool TryGetMapped(JsonElement value, out JsonEncodedText encoded)
    {
        var s = value.GetString();
        if (s is not null && _map.TryGetValue(s, out encoded))
        {
            return true;
        }

        encoded = default;
        return false;
    }
}
