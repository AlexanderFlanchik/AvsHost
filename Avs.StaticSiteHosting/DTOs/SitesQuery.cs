using Avs.StaticSiteHosting.Common;

namespace Avs.StaticSiteHosting.DTOs
{
    public class SitesQuery : PaginationParameters
    {
        public string OwnerId { get; set; }
    }
}