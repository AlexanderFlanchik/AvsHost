namespace Avs.StaticSiteHosting.ContentCreator.Models
{
    public class HtmlTreeRoot
    {
        public Head Head { get; set; } = new Head();
        public Body Body { get; set; } = new Body();
    }
}