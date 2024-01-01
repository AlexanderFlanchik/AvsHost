using System.Linq;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class HomePageModel
    {
        /// <summary>
        /// Total amount of user sites.
        /// </summary>
        public int TotalSites { get; set; }

        /// <summary>
        /// Amount of active user sites.
        /// </summary>
        public int ActiveSites { get; set; }
        
        // Total errors in sites during latest 24 hours.
        public int Errors { get; set; }
        
        /// <summary>
        /// Total site visits during 24 hours.
        /// </summary>        
        public int TotalSiteVisits { get; set; }

        /// <summary>
        /// Storage usage by site.
        /// </summary>
        public StorageUsedInfo[] StorageUsedInfos { get; set; }

        /// <summary>
        /// Total storage usage in bytes.
        /// </summary>
        public decimal TotalContentSize => StorageUsedInfos?.Sum(i => i.Size) ?? 0;
    }

    public class StorageUsedInfo
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public decimal Size { get; set; }
    }
}