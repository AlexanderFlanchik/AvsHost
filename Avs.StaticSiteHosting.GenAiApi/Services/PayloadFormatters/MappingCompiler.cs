using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// Compiles a mapping definition (JSON document) into an immutable <see cref="CompiledMapping"/>.
/// Compilation is done ONCE at startup; the resulting tree is reused for every transformation,
/// so no schema-parsing cost is paid on the hot path.
///
/// Template rules (applied recursively to every value in <c>template</c>):
///   1. An object that contains any reserved <c>$</c>-prefixed key is interpreted as a DIRECTIVE.
///      Supported directives: <c>$from</c>, <c>$const</c>, <c>$array</c>, <c>$find</c>.
///   2. A plain JSON string starting with "$." or "@." is interpreted as a PATH reference (sugar
///      for <c>{ "$from": "..." }</c>).
///   3. Anything else (object literal, array literal, primitive) is emitted as-is.
/// </summary>
public static class MappingCompiler
{
    private const string KeyFrom = "$from";
    private const string KeyConst = "$const";
    private const string KeyArray = "$array";
    private const string KeyFind = "$find";
    private const string KeySum = "$sum";
    private const string KeyStringify = "$stringify";
    private const string KeyDefault = "$default";
    private const string KeyValueMap = "$valueMap";
    private const string KeySkipIfMissing = "$skipIfMissing";
    private const string KeyWhere = "$where";
    private const string KeyItem = "$item";
    private const string KeySelect = "$select";
    private const string KeyNe = "$ne";
    private const string KeyPrepend = "$prepend";
    private const string KeyAppend = "$append";
    private const string KeyIf = "$if";

    /// <summary>
    /// Compile a mapping definition from its UTF-8 JSON bytes.
    /// </summary>
    public static CompiledMapping Compile(ReadOnlySpan<byte> mappingJson, string mappingId)
    {
        using var doc = JsonDocument.Parse(mappingJson.ToArray());
        return Compile(doc.RootElement, mappingId);
    }

    /// <summary>
    /// Compile a mapping definition from a string.
    /// </summary>
    public static CompiledMapping Compile(string mappingJson, string mappingId)
    {
        using var doc = JsonDocument.Parse(mappingJson);
        return Compile(doc.RootElement, mappingId);
    }

    private static CompiledMapping Compile(JsonElement root, string mappingId)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("Mapping definition must be an object.");
        }

        string name = root.TryGetProperty("name", out var nameEl) && nameEl.ValueKind == JsonValueKind.String
            ? nameEl.GetString()!
            : "(unnamed)";

        string? description = root.TryGetProperty("description", out var descEl) && descEl.ValueKind == JsonValueKind.String
            ? descEl.GetString()
            : null;

        if (!root.TryGetProperty("template", out var template))
        {
            throw new InvalidOperationException("Mapping definition must contain a 'template' field.");
        }

        var rootNode = CompileNode(template);
        return new CompiledMapping(name,  mappingId, description, rootNode);
    }

    private static IMappingNode CompileNode(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                return CompileObjectOrDirective(el);
            case JsonValueKind.Array:
                return CompileArrayLiteral(el);
            case JsonValueKind.String:
                {
                    var s = el.GetString()!;
                    if (IsPathLiteral(s))
                    {
                        return new FromNode(JsonPath.Parse(s), defaultRawJson: null, valueMap: null,
                            skipIfMissing: false);
                    }

                    return new ConstantNode(RawBytes(el));
                }
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                return new ConstantNode(RawBytes(el));
            default:
                throw new InvalidOperationException($"Unsupported template element: {el.ValueKind}.");
        }
    }

    private static bool IsPathLiteral(string s)
        => s.Length >= 2 && (s[0] == '$' || s[0] == '@') && (s.Length == 1 || s[1] == '.' || s[1] == '[');

    private static IMappingNode CompileObjectOrDirective(JsonElement obj)
    {
        // Detect directive
        string? directive = null;
        foreach (var prop in obj.EnumerateObject())
        {
            if (prop.Name.Length <= 0 || prop.Name[0] != '$')
            {
                continue;
            }
            
            if (prop.Name is not (KeyFrom or KeyConst or KeyArray or KeyFind or KeySum or KeyStringify))
            {
                continue;
            }
                
            if (directive is not null)
            {
                throw new InvalidOperationException(
                    $"Object contains multiple top-level directives: '{directive}' and '{prop.Name}'.");
            }

            directive = prop.Name;
        }

        return directive switch
        {
            KeyFrom => CompileFrom(obj),
            KeyConst => CompileConst(obj),
            KeyArray => CompileArrayFromSource(obj),
            KeyFind => CompileFind(obj),
            KeySum => CompileSum(obj),
            KeyStringify => CompileStringify(obj),
            null => CompileObjectLiteral(obj),
            _ => throw new InvalidOperationException($"Unknown directive '{directive}'.")
        };
    }

    private static IMappingNode CompileSum(JsonElement obj)
    {
        var arr = obj.GetProperty(KeySum);
        if (arr.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("$sum must be an array of path strings.");
        }

        var paths = new PathSegment[arr.GetArrayLength()][];
        int i = 0;
        foreach (var item in arr.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException("$sum array items must be path strings.");
            }

            paths[i++] = JsonPath.Parse(item.GetString()!);
        }
        return new SumNode(paths);
    }

    private static IMappingNode CompileObjectLiteral(JsonElement obj)
    {
        var names = new List<JsonEncodedText>(obj.GetPropertyCount());
        var children = new List<IMappingNode>(obj.GetPropertyCount());
        foreach (var prop in obj.EnumerateObject())
        {
            // Skip metadata-ish fields; template authors may stash $description etc.
            if (prop.Name.Length > 0 && prop.Name[0] == '$')
            {
                continue;
            }

            names.Add(JsonEncodedText.Encode(prop.Name));
            children.Add(CompileNode(prop.Value));
        }
        return new ObjectNode(names.ToArray(), children.ToArray());
    }

    private static IMappingNode CompileArrayLiteral(JsonElement arr)
    {
        var items = new List<IMappingNode>(arr.GetArrayLength());
        foreach (var item in arr.EnumerateArray())
        {
            items.Add(CompileNode(item));
        }
        
        return new ArrayLiteralNode(items.ToArray());
    }

    private static IMappingNode CompileFrom(JsonElement obj)
    {
        var path = JsonPath.Parse(RequireString(obj, KeyFrom));
        byte[]? defaultRaw = obj.TryGetProperty(KeyDefault, out var def) ? RawBytes(def) : null;
        bool skipIfMissing = obj.TryGetProperty(KeySkipIfMissing, out var sm) && sm.ValueKind == JsonValueKind.True;
        ValueMap? valueMap = null;
        if (obj.TryGetProperty(KeyValueMap, out var vm))
        {
            if (vm.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("$valueMap must be an object.");
            }

            var dict = new Dictionary<string, JsonEncodedText>(vm.GetPropertyCount(), StringComparer.Ordinal);
            foreach (var p in vm.EnumerateObject())
            {
                if (p.Value.ValueKind != JsonValueKind.String)
                {
                    throw new InvalidOperationException("$valueMap values must be strings.");
                }

                dict[p.Name] = JsonEncodedText.Encode(p.Value.GetString()!);
            }
            valueMap = new ValueMap(dict);
        }
        return new FromNode(path, defaultRaw, valueMap, skipIfMissing);
    }

    private static IMappingNode CompileConst(JsonElement obj)
    {
        var value = obj.GetProperty(KeyConst);
        return new ConstantNode(RawBytes(value));
    }

    private static IMappingNode CompileArrayFromSource(JsonElement obj)
    {
        var path = JsonPath.Parse(RequireString(obj, KeyArray));
        var path2 = StripTrailingWildcard(path);

        if (!obj.TryGetProperty(KeyItem, out var item))
        {
            throw new InvalidOperationException("$array directive requires a '$item' template.");
        }

        var itemNode = CompileNode(item);

        WhereFilter? filter = obj.TryGetProperty(KeyWhere, out var w) ? CompileWhere(w) : null;
        var prepend = CompileExtras(obj, KeyPrepend);
        var append = CompileExtras(obj, KeyAppend);

        return new ArrayFromSourceNode(path2, itemNode, filter, prepend, append);
    }

    private static ExtraItem[] CompileExtras(JsonElement obj, string key)
    {
        if (!obj.TryGetProperty(key, out var arr))
        {
            return Array.Empty<ExtraItem>();
        }

        if (arr.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"{key} must be an array of templates.");
        }

        var result = new ExtraItem[arr.GetArrayLength()];
        int i = 0;
        foreach (var el in arr.EnumerateArray())
        {
            PathSegment[]? condition = null;
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(KeyIf, out var ifEl))
            {
                if (ifEl.ValueKind != JsonValueKind.String)
                {
                    throw new InvalidOperationException("$if must be a path string.");
                }

                condition = JsonPath.Parse(ifEl.GetString()!);
            }
            
            // CompileNode descends into the object; CompileObjectLiteral already skips $-prefixed
            // metadata properties, so $if is naturally excluded from the emitted template.
            result[i++] = new ExtraItem(condition, CompileNode(el));
        }
        return result;
    }

    private static IMappingNode CompileFind(JsonElement obj)
    {
        var path = JsonPath.Parse(RequireString(obj, KeyFind));
        var path2 = StripTrailingWildcard(path);

        if (!obj.TryGetProperty(KeySelect, out var select))
        {
            throw new InvalidOperationException("$find directive requires a '$select' template.");
        }
        
        var selectNode = CompileNode(select);

        WhereFilter? filter = obj.TryGetProperty(KeyWhere, out var w) ? CompileWhere(w) : null;
        bool skipIfMissing = obj.TryGetProperty(KeySkipIfMissing, out var sm) && sm.ValueKind == JsonValueKind.True;
        
        return new FindNode(path2, filter, selectNode, skipIfMissing);
    }
    
    private static IMappingNode CompileStringify(JsonElement obj)
    {
        var path = JsonPath.Parse(RequireString(obj, KeyStringify));
        bool skipIfMissing = obj.TryGetProperty(KeySkipIfMissing, out var sm) && sm.ValueKind == JsonValueKind.True;
        
        return new JsonStringifyNode(path, skipIfMissing);
    }
    
    private static WhereFilter CompileWhere(JsonElement w)
    {
        if (w.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("$where must be an object.");
        }

        var list = new List<WhereFilter.Condition>(w.GetPropertyCount());
        foreach (var prop in w.EnumerateObject())
        {
            var fieldUtf8 = System.Text.Encoding.UTF8.GetBytes(prop.Name);
            if (prop.Value.ValueKind == JsonValueKind.Object && prop.Value.TryGetProperty(KeyNe, out var ne))
            {
                list.Add(new WhereFilter.Condition(fieldUtf8, RawBytes(ne), negate: true));
            }
            else
            {
                list.Add(new WhereFilter.Condition(fieldUtf8, RawBytes(prop.Value), negate: false));
            }
        }
        return new WhereFilter(list.ToArray());
    }

    private static PathSegment[] StripTrailingWildcard(PathSegment[] path)
    {
        if (path.Length > 0 && path[^1].Kind == PathSegmentKind.Wildcard)
        {
            var trimmed = new PathSegment[path.Length - 1];
            Array.Copy(path, trimmed, trimmed.Length);
            return trimmed;
        }
        return path;
    }

    private static string RequireString(JsonElement obj, string key)
    {
        if (!obj.TryGetProperty(key, out var v) || v.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"Directive requires string '{key}'.");
        }
        
        return v.GetString()!;
    }

    private static byte[] RawBytes(JsonElement el) => System.Text.Encoding.UTF8.GetBytes(el.GetRawText());
}
