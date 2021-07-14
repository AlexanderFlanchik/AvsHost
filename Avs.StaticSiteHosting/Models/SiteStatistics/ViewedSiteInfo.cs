using System;

namespace Avs.StaticSiteHosting.Web.Models.SiteStatistics
{
    public class ViewedSiteInfo : BaseEntity
    {
        public ViewedSiteInfo()
        {
            Name = "Viewed";
        }

        public DateTime ViewedTimestamp { get; set; }
        public string SiteId { get; set; }
        public string Visitor { get; set; } // The person IP how viewed the site
    }
}