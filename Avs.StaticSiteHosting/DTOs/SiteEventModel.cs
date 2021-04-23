using Avs.StaticSiteHosting.Web.Models;
using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class SiteEventModel
    {
        public string EventId { get; set; }
        public string SiteName { get; set; }
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
    }
}