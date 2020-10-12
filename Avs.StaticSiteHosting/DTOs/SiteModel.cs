using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class SiteModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? LaunchedOn { get; set; }
        public bool IsActive { get; set; }
        public string LandingPage { get; set; }
    }
}