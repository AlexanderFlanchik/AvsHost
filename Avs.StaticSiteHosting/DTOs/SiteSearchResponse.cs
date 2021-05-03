using Avs.StaticSiteHosting.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class SitesSearchResponse
    {
        public SiteSearchResponse[] Sites { get; }
        
        public SitesSearchResponse(IEnumerable<Site> sites)
        {
            Sites = sites.Select(s => new SiteSearchResponse { Id = s.Id, Name = s.Name }).ToArray();
        }
    }

    public class SiteSearchResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}