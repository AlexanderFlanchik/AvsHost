namespace Avs.StaticSiteHosting.Web.Models
{
    public class AppSettings : BaseEntity
    {
        public string Value { get; set; }
        public string Description { get; set; }
    }
}