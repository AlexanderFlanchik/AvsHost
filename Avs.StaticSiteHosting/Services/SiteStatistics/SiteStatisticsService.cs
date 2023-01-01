using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Models.SiteStatistics;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.SiteStatistics
{
    public interface ISiteStatisticsService
    {
        Task<bool> MarkSiteAsViewed(string siteId, string visitor);
        Task<int> GetTotalSiteVisits(string ownerId);
        Task<IEnumerable<ViewedSiteInfoModel>> GetLatestSiteVisits(string ownerId, int take = 5);
    }

    public class SiteStatisticsService : ISiteStatisticsService
    {
        private readonly IMongoCollection<ViewedSiteInfo> _viewedSiteInfos;
        private readonly ISiteService _siteService;
        private readonly IMongoCollection<Site> _sites;

        public SiteStatisticsService(MongoEntityRepository entityRepository, ISiteService siteService)
        {
            _viewedSiteInfos = entityRepository.GetEntityCollection<ViewedSiteInfo>(GeneralConstants.SITE_VIEWED_INFO_COLLECTION);
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _sites = entityRepository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);
        }

        public async Task<int> GetTotalSiteVisits(string ownerId)
        {
            var siteIds = await _siteService.GetSiteIdsByOwner(ownerId);
           
            var dto = DateTime.UtcNow;
            var df = dto.AddHours(-24);

            var dtoFilter = new FilterDefinitionBuilder<ViewedSiteInfo>().Lte(d => d.ViewedTimestamp, dto);
            var dfFilter = new FilterDefinitionBuilder<ViewedSiteInfo>().Gte(d => d.ViewedTimestamp, df);
            var idsFilter = new FilterDefinitionBuilder<ViewedSiteInfo>().In(s => s.SiteId, siteIds);

            var datesFilter = dfFilter & dtoFilter;
            var resultFilter = idsFilter & datesFilter;

            var count = await _viewedSiteInfos.Find(resultFilter).CountDocumentsAsync();
            
            return (int)count;
        }

        public async Task<bool> MarkSiteAsViewed(string siteId, string visitor)
        {
            var now = DateTime.UtcNow;
            var df = now.AddMinutes(-1);

            var filterById = new FilterDefinitionBuilder<ViewedSiteInfo>().Eq(f => f.SiteId, siteId);
            var filterByDate = new FilterDefinitionBuilder<ViewedSiteInfo>().Gte(f => f.ViewedTimestamp, df);
            var filterByVisitor = new FilterDefinitionBuilder<ViewedSiteInfo>().Eq(f => f.Visitor, visitor);
            var viewedEntriesQuery = await _viewedSiteInfos.FindAsync(filterById & filterByDate & filterByVisitor);
            var alreadyExists = await viewedEntriesQuery.AnyAsync();

            if (!alreadyExists)
            {
                await _viewedSiteInfos.InsertOneAsync(
                    new ViewedSiteInfo 
                        { 
                            SiteId = siteId, 
                            ViewedTimestamp = now,
                            Visitor = visitor
                        }
                );

                return true;
            }

            return false;
        }

        public async Task<IEnumerable<ViewedSiteInfoModel>> GetLatestSiteVisits(string ownerId, int take = 5)
        {
            var sitesFilter = new FilterDefinitionBuilder<Site>().Eq(o => o.CreatedBy.Id, ownerId);
            var sitesProjection = new ProjectionDefinitionBuilder<Site>().Expression(s => s.Id);
            var siteIds = await _sites.Find(sitesFilter).Project(sitesProjection).ToListAsync();

            var dateStart = DateTime.UtcNow.AddHours(-24);
            var filter = Builders<ViewedSiteInfo>.Filter.In(s => s.SiteId, siteIds)
                    & Builders<ViewedSiteInfo>.Filter.Gte(vs => vs.ViewedTimestamp, dateStart);

            var aggr = _viewedSiteInfos.Aggregate()                    
                    .Lookup<Site, ViewedSiteInfo>(GeneralConstants.SITES_COLLECTION, "SiteId", "_id", "Sites")                    
                    .Match(filter)
                    .Group(
                            v => v.SiteId, 
                            v => new ViewedSiteInfo 
                                    { 
                                        SiteId = v.Key, 
                                        Sites = v.First().Sites,
                                        ViewedTimestamp = v.Max(d => d.ViewedTimestamp)
                                    }
                    )
                    .SortByDescending(s => s.ViewedTimestamp)
                    .Limit(take);
      
            var resultsList = (await aggr.ToListAsync())
                    .Select(vi =>
                        new ViewedSiteInfoModel
                        {
                            SiteId = vi.Sites.FirstOrDefault()?.Id,
                            SiteName = vi.Sites.FirstOrDefault()?.Name,
                            Visit = vi.ViewedTimestamp
                        });

            return resultsList;
        }
    }
}