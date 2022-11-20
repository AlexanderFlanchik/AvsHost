using Avs.StaticSiteHosting.Web.Common;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class SitesQuery : PaginationParameters
    {
        public string OwnerId { get; set; }
        public string[] TagIds { get; set; }
    }
}