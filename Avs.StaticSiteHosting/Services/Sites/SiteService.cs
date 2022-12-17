using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Models.Identity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tag = Avs.StaticSiteHosting.Web.Models.Tags.Tag;

namespace Avs.StaticSiteHosting.Web.Services
{
    public class SiteService : ISiteService
    {
        private readonly IMongoCollection<Site> _sites;

        public SiteService(MongoEntityRepository entityRepository)
            => _sites = entityRepository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);

        public async Task<IEnumerable<string>> GetSiteIdsByOwner(string ownerId)
        {
            var sitesFilter = new FilterDefinitionBuilder<Site>().Eq(s => s.CreatedBy.Id, ownerId);
            var idProjection = Builders<Site>.Projection.Expression(s => s.Id);
            var siteIds = await _sites.Find(sitesFilter).Project(idProjection).ToListAsync();

            return siteIds;
        }

        public async Task<IEnumerable<SiteModel>> GetSitesAsync(SitesQuery query)
        {
            var filterBuilder = new FilterDefinitionBuilder<Site>();
            var filter = filterBuilder.Empty;
            
            if (!string.IsNullOrEmpty(query.OwnerId))
            {
                var filterByOwner = filterBuilder.Where(r => r.CreatedBy.Id == query.OwnerId);
                filter = filterBuilder.And(filter, filterByOwner);
            }
            
            if (!string.IsNullOrEmpty(query.SiteName)) 
            {
                var filterBySiteName = filterBuilder.Where(r => r.Name.Contains(query.SiteName));
                filter = filterBuilder.And(filter, filterBySiteName);
            }

            if (query.TagIds is not null && query.TagIds.Any())
            {
                var filterByTags = filterBuilder.Where(r => r.TagIds != null && r.TagIds.Any(t => query.TagIds.Contains(t.Id)));
                filter = filterBuilder.And(filter, filterByTags);
            }

            SortDefinition<Site> sort = null;
            if (!string.IsNullOrEmpty(query.SortField))
            {
                var sortDefBuilder = new SortDefinitionBuilder<Site>();
                switch (query.SortOrder)
                {
                    case SortOrder.Asc:
                        sort = sortDefBuilder.Ascending(query.SortField);
                        break;
                    case SortOrder.Desc:
                        sort = sortDefBuilder.Descending(query.SortField);
                        break;               
                }
            }

            var sitesQuery = _sites.Aggregate().Match(filter);
            if (sort is not null)
            {
                sitesQuery = sitesQuery.Sort(sort);
            }
                
            sitesQuery = sitesQuery.Skip(query.PageSize * (query.Page - 1))
                .Limit(query.PageSize)
                .Lookup<Tag, Site>(GeneralConstants.TAGS_COLLECTION, "TagIds._id", "_id", "Tags");

            var sitesList = await sitesQuery.ToListAsync();

            return sitesList.Select(s => new SiteModel 
                { 
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    LandingPage = s.LandingPage,
                    LaunchedOn = s.LaunchedOn,
                    Owner = new UserModel { Id = s.CreatedBy.Id, UserName = s.CreatedBy.Name },
                    IsActive = s.IsActive,
                    Tags = s.Tags?.Select(t => new TagModel(t.Id, t.Name, t.BackgroundColor, t.TextColor)).ToArray()
                });
        }

        public async Task<int> GetSitesAmountAsync(string ownerId = null)
         => await GetSitesAmount(null, ownerId);

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

        public async Task<Site> GetSiteByNameAsync(string siteName)
        {
            var siteCursor = await _sites.FindAsync(s => s.Name == siteName).ConfigureAwait(false);
            
            return await siteCursor.FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<SitesSearchResponse> SearchSitesByName(string name, string ownerId = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new SitesSearchResponse(new Site[] { });
            }

            var sitesFilter = new FilterDefinitionBuilder<Site>().Where(s => s.Name.ToLowerInvariant().Contains(name.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(ownerId))
            {
                sitesFilter &= new FilterDefinitionBuilder<Site>().Eq(s => s.CreatedBy.Id, ownerId);
            }

            var sitesQuery = await _sites.FindAsync(sitesFilter, new FindOptions<Site> { Limit = 50 });
           
            return new SitesSearchResponse(await sitesQuery.ToListAsync());
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
                                      .Set(s => s.Mappings, siteToUpdate.Mappings)
                                      .Set(s => s.LandingPage, siteToUpdate.LandingPage)
                                      .Set(s => s.TagIds, siteToUpdate.TagIds);
            
            await _sites.UpdateOneAsync(filter, update).ConfigureAwait(false);
        }

        public async Task<bool> ToggleSiteStatusAsync(Site site)
        {
            site.IsActive = !site.IsActive;
            await UpdateSiteAsync(site).ConfigureAwait(false);

            return site.IsActive;
        }

        public async Task DeleteSiteAsync(string siteId)
        {
            var filterBuilder = new FilterDefinitionBuilder<Site>();
            var filter = filterBuilder.Where(s => s.Id == siteId);

            await _sites.DeleteOneAsync(filter).ConfigureAwait(false);
        }

        public async Task<int> GetActiveSitesAmountAsync(string ownerId = null)
         => await GetSitesAmount(true, ownerId);

        public async Task UpdateSitesStatusAsync(string ownerId, UserStatus status)
        {
            await _sites.UpdateManyAsync(new FilterDefinitionBuilder<Site>().Where(s => s.CreatedBy.Id == ownerId),
                new UpdateDefinitionBuilder<Site>().Set(s => s.CreatedBy.Status, status)
             ).ConfigureAwait(false);
        }

        private async Task<int> GetSitesAmount(bool? isActive, string ownerId)
        {
            IAsyncCursor<Site> siteCursor = null;
            if (!string.IsNullOrEmpty(ownerId))
            {
                siteCursor = isActive.HasValue ? await _sites.FindAsync(
                        s => s.IsActive == isActive.Value && 
                            s.CreatedBy != null && 
                            s.CreatedBy.Id == ownerId
                            ).ConfigureAwait(false)
                     : 
                     await _sites.FindAsync(
                        s => s.CreatedBy != null &&
                             s.CreatedBy.Id == ownerId).ConfigureAwait(false);
            }
            else
            {
                siteCursor = isActive.HasValue ? await _sites.FindAsync(s => s.IsActive == isActive.Value).ConfigureAwait(false)
                     :
                     await _sites.FindAsync(s => true).ConfigureAwait(false);
            }

            return siteCursor.ToEnumerable().Count();
        }
    }
}