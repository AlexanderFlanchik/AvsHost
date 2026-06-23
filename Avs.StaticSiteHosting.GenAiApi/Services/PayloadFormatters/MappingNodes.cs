using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// A compiled mapping node. Each node writes directly to a <see cref="Utf8JsonWriter"/>,
/// without intermediate allocations or string materialization of source values.
/// </summary>
internal interface IMappingNode
{
    /// <summary>
    /// Emit a value (used by array items and the root of a compiled mapping).
    /// </summary>
    void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer);

    /// <summary>
    /// Emit as an object property: writes name + value, OR writes nothing if the node opts out
    /// (e.g. <c>$skipIfMissing</c>). Implemented this way so callers can short-circuit before
    /// committing the property name to the writer.
    /// </summary>
    void WriteProperty(JsonEncodedText name, JsonElement root, JsonElement current, Utf8JsonWriter writer);
}

/// <summary>
/// Default helper: most nodes always emit a value, so writing as a property is just name + value.
/// </summary>
internal abstract class MappingNode : IMappingNode
{
    public abstract void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer);

    public virtual void WriteProperty(JsonEncodedText name, JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        writer.WritePropertyName(name);
        WriteValue(root, current, writer);
    }
}

/// <summary>
/// Object literal: <c>{ "key": child, ... }</c>. Property names are pre-encoded.
/// </summary>
internal sealed class ObjectNode : MappingNode
{
    private readonly JsonEncodedText[] _names;
    private readonly IMappingNode[] _children;

    public ObjectNode(JsonEncodedText[] names, IMappingNode[] children)
    {
        _names = names;
        _children = children;
    }

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        for (int i = 0; i < _children.Length; i++)
        {
            _children[i].WriteProperty(_names[i], root, current, writer);
        }
        writer.WriteEndObject();
    }
}

/// <summary>
/// Array literal: <c>[ child, ... ]</c> where each child is a fixed template.
/// </summary>
internal sealed class ArrayLiteralNode : MappingNode
{
    private readonly IMappingNode[] _items;

    public ArrayLiteralNode(IMappingNode[] items) => _items = items;

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        writer.WriteStartArray();
        for (int i = 0; i < _items.Length; i++)
        {
            _items[i].WriteValue(root, current, writer);
        }
        writer.WriteEndArray();
    }
}

/// <summary>
/// Pre-serialized literal value (null, bool, number, string). Emits raw UTF-8 token bytes.
/// </summary>
internal sealed class ConstantNode : MappingNode
{
    private readonly byte[] _rawJson;

    public ConstantNode(byte[] rawJson) => _rawJson = rawJson;

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        writer.WriteRawValue(_rawJson, skipInputValidation: true);
    }
}

/// <summary>
/// Pulls a value from the source via a compiled path. Optional default + valueMap + skipIfMissing.
/// </summary>
internal sealed class FromNode : MappingNode
{
    private readonly PathSegment[] _path;
    private readonly byte[]? _defaultRawJson;
    private readonly ValueMap? _valueMap;
    private readonly bool _skipIfMissing;

    public FromNode(PathSegment[] path, byte[]? defaultRawJson, ValueMap? valueMap, bool skipIfMissing)
    {
        _path = path;
        _defaultRawJson = defaultRawJson;
        _valueMap = valueMap;
        _skipIfMissing = skipIfMissing;
    }

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        if (JsonPath.TryResolve(root, current, _path, out var el) && el.ValueKind != JsonValueKind.Undefined)
        {
            EmitMappedOrRaw(el, writer);
            return;
        }
        if (_defaultRawJson is not null)
        {
            writer.WriteRawValue(_defaultRawJson, skipInputValidation: true);
            return;
        }
        writer.WriteNullValue();
    }

    public override void WriteProperty(JsonEncodedText name, JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        if (JsonPath.TryResolve(root, current, _path, out var el) && el.ValueKind != JsonValueKind.Undefined)
        {
            writer.WritePropertyName(name);
            EmitMappedOrRaw(el, writer);
            return;
        }
        if (_defaultRawJson is not null)
        {
            writer.WritePropertyName(name);
            writer.WriteRawValue(_defaultRawJson, skipInputValidation: true);
            return;
        }
        if (_skipIfMissing) return;

        writer.WritePropertyName(name);
        writer.WriteNullValue();
    }

    private void EmitMappedOrRaw(JsonElement el, Utf8JsonWriter writer)
    {
        if (_valueMap is { } map && el.ValueKind == JsonValueKind.String)
        {
            if (map.TryGetMapped(el, out var encoded))
            {
                writer.WriteStringValue(encoded);
                return;
            }
        }
        el.WriteTo(writer);
    }
}

/// <summary>
/// Serializes a source element as a JSON string literal.
/// </summary>
internal sealed class JsonStringifyNode : MappingNode
{
    private readonly PathSegment[] _path;
    private readonly bool _skipIfMissing;

    public JsonStringifyNode(PathSegment[] path, bool skipIfMissing)
    {
        _path = path;
        _skipIfMissing = skipIfMissing;
    }

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        if (JsonPath.TryResolve(root, current, _path, out var el) && el.ValueKind != JsonValueKind.Undefined)
        {
            writer.WriteStringValue(el.GetRawText());
            return;
        }

        writer.WriteNullValue();
    }

    public override void WriteProperty(JsonEncodedText name, JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        if (JsonPath.TryResolve(root, current, _path, out var el) && el.ValueKind != JsonValueKind.Undefined)
        {
            writer.WritePropertyName(name);
            writer.WriteStringValue(el.GetRawText());
            
            return;
        }

        if (_skipIfMissing)
        {
            return;
        }

        writer.WritePropertyName(name);
        writer.WriteNullValue();
    }
    
    private static string SerializeCompact(JsonElement el)
    {
        var buffer = new ArrayBufferWriter<byte>(256);
        var options = new JsonWriterOptions 
        { 
            Indented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        using var writer = new Utf8JsonWriter(buffer, options);
        el.WriteTo(writer);
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
    }
}


/// <summary>
/// One fixed array item, optionally guarded by an <c>$if</c> condition (a source path that must
/// resolve to a non-null / non-undefined value for the item to be emitted).
/// </summary>
internal readonly struct ExtraItem
{
    public ExtraItem(PathSegment[]? condition, IMappingNode template)
    {
        Condition = condition;
        Template = template;
    }

    public PathSegment[]? Condition { get; }
    public IMappingNode Template { get; }

    public bool ConditionHolds(JsonElement root, JsonElement current)
    {
        if (Condition is null) return true;
        if (!JsonPath.TryResolve(root, current, Condition, out var probe)) return false;
        return probe.ValueKind is not (JsonValueKind.Undefined or JsonValueKind.Null);
    }
}

/// <summary>
/// Iterates over a source array and emits an item template per element.
/// Optional <c>$where</c> filter restricts which items are emitted.
/// Optional <c>$prepend</c> / <c>$append</c> splice fixed (optionally conditional) items
/// around the iterated values; each is rendered against the document root.
/// </summary>
internal sealed class ArrayFromSourceNode : MappingNode
{
    private readonly PathSegment[] _arrayPath;
    private readonly IMappingNode _itemTemplate;
    private readonly WhereFilter? _filter;
    private readonly ExtraItem[] _prepend;
    private readonly ExtraItem[] _append;

    public ArrayFromSourceNode(
        PathSegment[] arrayPath,
        IMappingNode itemTemplate,
        WhereFilter? filter,
        ExtraItem[] prepend,
        ExtraItem[] append)
    {
        _arrayPath = arrayPath;
        _itemTemplate = itemTemplate;
        _filter = filter;
        _prepend = prepend;
        _append = append;
    }

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        writer.WriteStartArray();

        WriteExtras(_prepend, root, current, writer);

        if (JsonPath.TryResolve(root, current, _arrayPath, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                if (_filter is not null && !_filter.Matches(item)) continue;
                _itemTemplate.WriteValue(root, item, writer);
            }
        }

        WriteExtras(_append, root, current, writer);

        writer.WriteEndArray();
    }

    private static void WriteExtras(ExtraItem[] extras, JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        for (int i = 0; i < extras.Length; i++)
        {
            ref readonly var x = ref extras[i];
            if (!x.ConditionHolds(root, current))
            {
                continue;
            }
            
            x.Template.WriteValue(root, current, writer);
        }
    }
}

/// <summary>
/// Sums a fixed list of source paths and emits a single JSON number. Missing paths and
/// non-numeric values contribute 0. Stays in int64 arithmetic as long as every contributing
/// value is an integer; falls back to double on the first non-integer.
/// </summary>
internal sealed class SumNode : MappingNode
{
    private readonly PathSegment[][] _paths;

    public SumNode(PathSegment[][] paths) => _paths = paths;

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        long iSum = 0;
        double dSum = 0;
        bool allIntegers = true;

        for (int i = 0; i < _paths.Length; i++)
        {
            if (!JsonPath.TryResolve(root, current, _paths[i], out var el))
            {
                continue;
            }

            if (el.ValueKind != JsonValueKind.Number)
            {
                continue;
            }

            if (allIntegers && el.TryGetInt64(out long iv))
            {
                iSum += iv;
                dSum += iv;
            }
            else
            {
                allIntegers = false;
                dSum += el.GetDouble();
            }
        }

        if (allIntegers)
        {
            writer.WriteNumberValue(iSum);
        }
        else
        {
            writer.WriteNumberValue(dSum);
        }
    }
}

/// <summary>
/// Finds the first source array element matching an optional filter, then renders a sub-template
/// (or pulls a sub-path) against it.
/// </summary>
internal sealed class FindNode : MappingNode
{
    private readonly PathSegment[] _arrayPath;
    private readonly WhereFilter? _filter;
    private readonly IMappingNode _selector;
    private readonly bool _skipIfMissing;

    public FindNode(PathSegment[] arrayPath, WhereFilter? filter, IMappingNode selector, bool skipIfMissing)
    {
        _arrayPath = arrayPath;
        _filter = filter;
        _selector = selector;
        _skipIfMissing = skipIfMissing;
    }

    public override void WriteValue(JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        if (TryFind(root, current, out var found))
        {
            _selector.WriteValue(root, found, writer);
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    public override void WriteProperty(JsonEncodedText name, JsonElement root, JsonElement current, Utf8JsonWriter writer)
    {
        if (TryFind(root, current, out var found))
        {
            writer.WritePropertyName(name);
            _selector.WriteValue(root, found, writer);
            return;
        }

        if (_skipIfMissing)
        {
            return;
        }

        writer.WritePropertyName(name);
        writer.WriteNullValue();
    }

    private bool TryFind(JsonElement root, JsonElement current, out JsonElement found)
    {
        if (JsonPath.TryResolve(root, current, _arrayPath, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                if (_filter is not null && !_filter.Matches(item))
                {
                    continue;
                }
                
                found = item;
                return true;
            }
        }
        
        found = default;
        return false;
    }
}