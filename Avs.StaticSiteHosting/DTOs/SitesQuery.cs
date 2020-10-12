using Avs.StaticSiteHosting.Web.Common;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class SitesQuery : PaginationParameters
    {
        public string OwnerId { get; set; }
    }
}