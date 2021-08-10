using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ViewedSiteInfoModel
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public DateTime Visit { get; set; }
    }
}