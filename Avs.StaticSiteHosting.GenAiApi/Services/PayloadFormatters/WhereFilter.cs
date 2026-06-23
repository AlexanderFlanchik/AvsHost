using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// Lightweight predicate for <c>$where</c> conditions. Each condition compares a field of the
/// current item to a target value with <c>eq</c> or <c>ne</c> semantics. All conditions AND together.
/// </summary>
internal sealed class WhereFilter
{
    /// <summary>One field-level condition.</summary>
    public readonly struct Condition
    {
        public Condition(byte[] fieldUtf8, byte[] rawJsonValue, bool negate)
        {
            FieldUtf8 = fieldUtf8;
            RawJsonValue = rawJsonValue;
            Negate = negate;
        }

        public byte[] FieldUtf8 { get; }
        
        /// <summary>
        /// Pre-serialized JSON of the comparison value, used for raw-text comparison.
        /// </summary>
        public byte[] RawJsonValue { get; }
        public bool Negate { get; }
    }

    private readonly Condition[] _conditions;

    public WhereFilter(Condition[] conditions) => _conditions = conditions;

    public bool Matches(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object) return false;

        for (int i = 0; i < _conditions.Length; i++)
        {
            ref readonly var cond = ref _conditions[i];
            bool eq;
            if (!item.TryGetProperty(cond.FieldUtf8, out var prop))
            {
                eq = false;
            }
            else
            {
                // Compare via the raw token text. This avoids allocating a string for every comparison
                // and works uniformly for string/number/bool/null literals.
                eq = RawEquals(prop, cond.RawJsonValue);
            }

            if (eq == cond.Negate)
            {
                return false;
            }
        }
        return true;
    }

    private static bool RawEquals(JsonElement el, ReadOnlySpan<byte> rawJsonValue)
    {
        // For strings we can compare unquoted bytes directly to dodge escape-aware comparisons:
        if (el.ValueKind == JsonValueKind.String && rawJsonValue.Length >= 2 && rawJsonValue[0] == (byte)'"')
        {
            return el.ValueEquals(rawJsonValue[1..^1]);
        }
        
        // Fall back to raw-text equality for booleans / numbers / null.
        var raw = el.GetRawText();
        if (raw.Length != rawJsonValue.Length)
        {
            return false;
        }
        
        for (int i = 0; i < raw.Length; i++)
        {
            if ((byte)raw[i] == rawJsonValue[i])
            {
                continue;
            }
            
            return false;
        }
        
        return true;
    }
}
