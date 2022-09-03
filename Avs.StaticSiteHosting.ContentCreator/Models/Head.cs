namespace Avs.StaticSiteHosting.ContentCreator.Models
{
    public class Head
    {
        public string? Title { get; set; }
        public IList<Metadata> Metadatas { get; set; } = new List<Metadata>();
        public IList<Script> Scripts { get; set; } = new List<Script>();
        public IList<string> Styles { get; set; } = new List<string>();
        public IList<Link> Links { get; set; } = new List<Link>();
    }
}