using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
{
    public interface ISiteService
    {
        Task<IEnumerable<Site>> GetSitesAsync(SitesQuery query);
        Task<int> GetSitesAmountAsync(string ownerId = null);
    }
}
