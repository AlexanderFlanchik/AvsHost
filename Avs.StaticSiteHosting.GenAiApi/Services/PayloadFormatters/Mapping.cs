using System.Text.Json.Nodes;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public class Mapping
{
    private string[] _tokens = [];
    
    public string Name { get; set; } = null!;

    public string? SourcePath
    {
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            
            _tokens = value.Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x != "$")
                .ToArray();
        }
    }
    
    public string? DefaultValue { get; set; }
    public string[] Tokens  => _tokens;
    public Condition? Condition { get; set; }
    public JsonNode[]? AddItems { get; init; }
    public List<Mapping>? ItemMappings { get; set; }
    public List<Mapping>? ArrayMappings { get; set; }
    public bool IsObject => ItemMappings != null;
    public bool IsArray => ArrayMappings != null;
}