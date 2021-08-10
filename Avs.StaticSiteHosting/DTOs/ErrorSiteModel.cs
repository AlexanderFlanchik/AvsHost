using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ErrorSiteModel
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public DateTime Timestamp { get; set; }
    }
}