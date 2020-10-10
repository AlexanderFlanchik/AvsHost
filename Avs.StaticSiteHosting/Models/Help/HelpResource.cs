namespace Avs.StaticSiteHosting.Models
{
    public class HelpResource : BaseEntity
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}