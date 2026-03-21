namespace Avs.StaticSiteHosting.Web.DTOs;

public class SitemapResult
{
    public string SitemapId { get; private set; }
    public decimal SizeKb { get; private set; }
    public string Error { get; private set; }
    
    private SitemapResult() {}

    public static SitemapResult Success(string sitemapId, decimal size) =>
        new()
        {
            SitemapId = sitemapId,
            SizeKb = size
        };
    
    public static SitemapResult Fail(string error) => new() { Error =  error };
}