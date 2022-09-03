namespace Avs.StaticSiteHosting.ContentCreator.Models
{
    public class Body
    {
        public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public string? InnerHtml { get; set; }
    }
}