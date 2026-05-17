using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FilterConstants = Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters.JsonFormatterConstants.Filters;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public static class PayloadMapper
{
    public static string MapPayload(string inputJson, Mapping[] mappings, IDictionary<string, string> contextValues)
    {
        using var mappedStream = (MemoryStream)MapPayloadStream(inputJson, mappings, contextValues);
        
        return Encoding.UTF8.GetString(mappedStream.ToArray());
    }

    public static Stream MapPayloadStream(string inputJson, Mapping[] mappings)
    {
        return MapPayloadStream(inputJson, mappings, new Dictionary<string, string>());
    }
    
    public static Stream MapPayloadStream(string inputJson, Mapping[] mappings, IDictionary<string, string> contextValues)
    {
        using var doc = JsonDocument.Parse(inputJson);
        
        return MapPayloadStreamInternal(doc, mappings, contextValues);
    }

    public static Stream MapPayloadStream(Stream inputJsonStream, Mapping[] mappings)
    {
        return MapPayloadStream(inputJsonStream, mappings, new Dictionary<string, string>());
    }
    
    public static Stream MapPayloadStream(Stream inputJsonStream, Mapping[] mappings, IDictionary<string, string> contextValues)
    {
        using var doc = JsonDocument.Parse(inputJsonStream);
        
        return MapPayloadStreamInternal(doc, mappings, contextValues);
    }

    private static Stream MapPayloadStreamInternal(JsonDocument doc, Mapping[] mappings, IDictionary<string, string> contextValues)
    {
        var ctx = new JsonRemappingContext() { Root = doc.RootElement, Items = contextValues };
        
        var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true
        });
        
        writer.WriteStartObject();

        foreach (var mapping in mappings)
        {
            ApplyMapping(doc.RootElement, writer, mapping, null, ctx);
        }
        
        writer.WriteEndObject();
        writer.Flush();
        stream.Position = 0;
        
        return stream;
    }

    private static void ApplyMapping(
        JsonElement root,
        Utf8JsonWriter writer,
        Mapping mapping,
        JsonElement? currentItem,
        JsonRemappingContext ctx)
    {
        var sourceElement = currentItem ?? root;
        if (!TryGetValue(sourceElement, mapping.Tokens, out JsonElement source))
        {
            if (!string.IsNullOrEmpty(mapping.DefaultValue))
            {
                writer.WritePropertyName(mapping.Name);
                
                var value = mapping.DefaultValue ?? string.Empty;
                foreach (var item in ctx.Items)
                {
                    const string br1 = "{{", br2 = "}}";
                    value = value.Replace($"{br1}{item.Key}{br2}", item.Value);
                }
                
                writer.WriteStringValue(value);
            }

            return;
        }

        if (mapping.Condition is not null)
        {
            bool isConditionSatisfied;
            bool sourceChecked = mapping.Condition.Tokens.SequenceEqual(mapping.Tokens);
            
            if (mapping.Condition.Tokens.Length > 0 &&
                !sourceChecked && 
                TryGetValue(sourceElement, mapping.Condition.Tokens, out var conditionTarget))
            {
                isConditionSatisfied = EvaluateCondition(mapping.Condition, conditionTarget);
            }
            else
            {
                isConditionSatisfied = EvaluateCondition(mapping.Condition, source);
            }

            if (!isConditionSatisfied)
            {
                return;
            }
        }

        switch (source.ValueKind)
        {
            case JsonValueKind.Array when mapping.IsArray:
                WriteArray(writer, mapping, source, ctx);
                break;
            case JsonValueKind.Object when mapping.IsObject:
                WriteObject(writer, mapping, source, ctx);
                break;
            default:
                writer.WritePropertyName(mapping.Name);
                source.WriteTo(writer);
                break;
        }
    }
    
    private static void WriteObject(
        Utf8JsonWriter writer,
        Mapping mapping,
        JsonElement source,
        JsonRemappingContext ctx)
    {
        writer.WritePropertyName(mapping.Name);
        writer.WriteStartObject();

        foreach (var child in mapping.ItemMappings!)
        {
            ApplyMapping(source, writer, child, null, ctx);
        }

        writer.WriteEndObject();
    }
    
    private static void WriteArray(
        Utf8JsonWriter writer,
        Mapping mapping,
        JsonElement array,
        JsonRemappingContext ctx)
    {
        writer.WritePropertyName(mapping.Name);
        writer.WriteStartArray();
        
        if (mapping.AddItems is not null)
        {
            foreach (var newItem in mapping.AddItems)
            {
                ReplacePlaceholders(newItem, ctx.Root);
                
                newItem.WriteTo(writer);
            }
        }

        foreach (var item in array.EnumerateArray())
        {
            if (ShouldSkipItem(mapping, array, item))
            {
                continue;
            }

            writer.WriteStartObject();

            foreach (var child in mapping.ArrayMappings!)
            {
                ApplyMapping(array, writer, child, item, ctx);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
    
    private static void ReplacePlaceholders(JsonNode item, JsonElement root)
    {
        var itemKind = item.GetValueKind();
       
        switch (itemKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in item.AsObject())
                {
                    if (prop.Value is null)
                    {
                        continue;
                    }
                    
                    ReplacePlaceholders(prop.Value, root);
                }
                break;
            case JsonValueKind.Array:
                foreach (var arrayItem in item.AsArray())
                {
                    if (arrayItem is null)
                    {
                        continue;
                    }
                    
                    ReplacePlaceholders(arrayItem, root);
                }
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                break;
            default:
                var itemValue = item.ToString();
                if (itemValue.Contains(JsonFormatterConstants.PathPattern))
                {
                    var parsedPath = itemValue.Replace(JsonFormatterConstants.PathPattern, string.Empty);
                    var tokens = parsedPath.Split('.').Where(t => t != "$").ToArray();

                    if (TryGetValue(root, tokens, out var valueNode))
                    {
                        item.ReplaceWith(valueNode.Clone());
                    }
                    else
                    {
                        var empty = JsonValue.Create(string.Empty);
                        item.ReplaceWith(empty);
                    }
                }
               
                break;
        }
    }
    
    private static bool ShouldSkipItem(Mapping mapping, JsonElement array, JsonElement item)
    {
        if (mapping.Condition is null || mapping.Condition.ItemFilter is null)
        {
            return false;
        }

        var itemFilter = mapping.Condition.ItemFilter;
        if (!TryGetValue(item, itemFilter.Tokens, out var targetItem))
        {
            return false;
        }

        bool check = true;
        switch (itemFilter.Operation)
        {
            case FilterConstants.Equal:
                check = itemFilter.Value == targetItem.GetString(); 
                break;
            case FilterConstants.NotEqual:
                check = itemFilter.Value != targetItem.GetString();
                break;
        }
        
        return !check;
    }
    
    private static bool TryGetValue(
        JsonElement element,
        string[] tokens,
        out JsonElement result)
    {
        result = element;

        foreach (var token in tokens)
        {
            var start = token.IndexOf('[');
            var end = token.IndexOf(']');
            
            if (start >= 0 && end >= 0)
            {
                var arrayName = token[..start];
                var indexSpan = token.AsSpan(start + 1, end - start - 1);
                if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty(arrayName, out result) || result.ValueKind != JsonValueKind.Array)
                {
                    return false;
                }

                if (int.TryParse(indexSpan, out var index) && index >= 0 && index < result.GetArrayLength())
                {
                    result = result[index];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty(token, out result))
                {
                    return false;
                }
            }
        }

        return true;
    }
    
    private static bool EvaluateCondition(Condition condition, JsonElement targetElement)
    {
        if (string.IsNullOrEmpty(condition.Operation) || string.IsNullOrEmpty(condition.Value))
        {
            return true;
        }

        bool check = true;

        switch (condition.Operation)
        {
            case FilterConstants.Equal:
                check = targetElement.GetString() == condition.Value;
                break;
            case FilterConstants.NotEqual:
                check = targetElement.GetString() != condition.Value;
                break;
        }
        
        return check;
    }
}