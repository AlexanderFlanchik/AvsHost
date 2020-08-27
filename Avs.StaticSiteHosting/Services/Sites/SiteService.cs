using Avs.StaticSiteHosting.Common;
using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
{
    public class SiteService : ISiteService
    {
        private readonly IMongoCollection<Site> _sites;

        public SiteService(MongoEntityRepository entityRepository)
        {
            _sites = entityRepository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);
        }

        public async Task<IEnumerable<Site>> GetSitesAsync(SitesQuery query)
        {
            var findOptions = new FindOptions<Site>() { Limit = query.PageSize, Skip = query.PageSize * (query.Page - 1) };
            var filterBuilder = new FilterDefinitionBuilder<Site>();
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(query.OwnerId))
            {
                var filterByOwner = filterBuilder.Where(r => r.CreatedBy.Id == query.OwnerId);
                filter = filterBuilder.And(filter, filterByOwner);
            }
                     
            if (!string.IsNullOrEmpty(query.SortField))
            {
                SortDefinition<Site> sort = null;
                var sortDefBuilder = new SortDefinitionBuilder<Site>();
                switch (query.SortOrder)
                {
                    case SortOrder.Asc:
                        sort = sortDefBuilder.Ascending(query.SortField);
                        break;
                    case SortOrder.Desc:
                        sort = sortDefBuilder.Descending(query.SortField);
                        break;
                    default:
                        sort = null;
                        break;
                }

                findOptions.Sort = sort;
            }

            var sitesCursor = await _sites.FindAsync(filter, findOptions).ConfigureAwait(false);
            var sitesList = await sitesCursor.ToListAsync().ConfigureAwait(false);

            return sitesList;
        }

        public async Task<int> GetSitesAmountAsync(string ownerId = null)
        {
            IAsyncCursor<Site> siteCursor = null;
            if (!string.IsNullOrEmpty(ownerId))
            {
                siteCursor = await _sites.FindAsync(s => s.CreatedBy != null && s.CreatedBy.Id == ownerId).ConfigureAwait(false);
            }
            else
            {
                siteCursor = await _sites.FindAsync(s => true).ConfigureAwait(false);
            }

            return siteCursor.ToEnumerable().Count();            
        }
    }
}