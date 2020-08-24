using Avs.StaticSiteHosting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
{
    public interface ISiteService
    {
        Task<IEnumerable<Site>> GetSitesByOwnerId(string ownerId);
    }
}
