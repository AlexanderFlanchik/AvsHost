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

        public async Task<bool> CheckSiteNameUsedAsync(string siteName, string siteId)
        {
            var findOptions = new FindOptions<Site>();
            var filterBuilder = new FilterDefinitionBuilder<Site>();

            var filter = filterBuilder.Where(s => 
                                s.Name == siteName && 
                                s.CreatedBy != null);

            if (!string.IsNullOrEmpty(siteId))
            {
                var siteIdFilter = filterBuilder.Where(r => r.Id != siteId);
                filter = filterBuilder.And(filter, siteIdFilter);
            }

            var siteCursor = await _sites.FindAsync(filter, findOptions).ConfigureAwait(false);
            var result = await siteCursor.AnyAsync().ConfigureAwait(false);

            return result;
        }

        public async Task<Site> CreateSiteAsync(Site newSite)
        {
            await _sites.InsertOneAsync(newSite).ConfigureAwait(false);
            return newSite;
        }

        public async Task<Site> GetSiteByIdAsync(string siteId)
        {
            var siteCursor = await _sites.FindAsync(s => s.Id == siteId).ConfigureAwait(false);
            
            return await siteCursor.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task UpdateSiteAsync(Site siteToUpdate)
        {
            var siteId = siteToUpdate.Id;
            var filterBuilder = new FilterDefinitionBuilder<Site>();

            var filter = filterBuilder.Where(s => s.Id == siteId);
            var updateBuilder = new UpdateDefinitionBuilder<Site>();

            var update = updateBuilder.Set(s => s.Name, siteToUpdate.Name)
                                      .Set(s => s.Description, siteToUpdate.Description)
                                      .Set(s => s.IsActive, siteToUpdate.IsActive)
                                      .Set(s => s.Mappings, siteToUpdate.Mappings);
            
            await _sites.UpdateOneAsync(filter, update).ConfigureAwait(false);
        }
    }
}