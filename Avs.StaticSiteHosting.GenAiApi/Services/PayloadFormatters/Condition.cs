namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public class Condition
{
    private string[] _tokens = [];
    public string? Operation { get; set; }
    public string[] Tokens => _tokens;

    public string? Path
    {
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            
            _tokens = value.Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x != "$")
                .ToArray();;
        }
    }
    
    public string? Value { get; set; }
    public ItemFilter? ItemFilter { get; set; }
}