namespace Avs.StaticSiteHosting.Tests.GenAiApi.PayloadFormatters;

public abstract class JsonRemapperTestsBase
{
    protected static async Task<string> GetContent(string subFolder, string fileName)
    {
        var baseFolder = AppDomain.CurrentDomain.BaseDirectory;
        var contentFile = Path.Combine(baseFolder, "GenAiApi", "PayloadFormatters", subFolder, fileName);
        
        if (!File.Exists(contentFile))
        {
            throw new FileNotFoundException($"Cannot find the content file '{fileName}'.");
        }
        
        var content = await File.ReadAllTextAsync(contentFile);
        
        return content;
    }
}