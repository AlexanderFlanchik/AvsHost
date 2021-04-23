using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.Models;
using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class EventLogsQuery : PaginationParameters
    {
        public string SiteId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public SiteEventType Type { get; set; }
        public string CurrentUserId { get; set; }

        public EventLogsQuery()
        {
            Page = 1;
            PageSize = 10;
        }
    }
}