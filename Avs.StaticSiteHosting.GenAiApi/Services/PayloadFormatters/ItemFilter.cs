namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public class ItemFilter
{
    public string Operation { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Value { get; set; } = null!;
    
    public string[] Tokens => Path.Split('.', StringSplitOptions.RemoveEmptyEntries);
}