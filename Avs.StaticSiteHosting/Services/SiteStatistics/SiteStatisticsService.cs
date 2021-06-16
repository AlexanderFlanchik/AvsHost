using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Models.SiteStatistics;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.SiteStatistics
{
    public interface ISiteStatisticsService
    {
        Task MarkSiteAsViewed(string siteId);
        Task<int> GetTotalSiteVisits(string ownerId);
    }

    public class SiteStatisticsService : ISiteStatisticsService
    {
        private readonly IMongoCollection<ViewedSiteInfo> _viewedSiteInfos;
        private readonly ISiteService _siteService;

        public SiteStatisticsService(MongoEntityRepository entityRepository, ISiteService siteService)
        {
            _viewedSiteInfos = entityRepository.GetEntityCollection<ViewedSiteInfo>(GeneralConstants.SITE_VIEWED_INFO_COLLECTION);
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
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

        public async Task MarkSiteAsViewed(string siteId)
        {
            var now = DateTime.UtcNow;
            var df = now.AddMinutes(-1);

            var filterById = new FilterDefinitionBuilder<ViewedSiteInfo>().Eq(f => f.SiteId, siteId);
            var filterByDate = new FilterDefinitionBuilder<ViewedSiteInfo>().Gte(f => f.ViewedTimestamp, df);
            
            var viewedEntriesQuery = await _viewedSiteInfos.FindAsync(filterById & filterByDate);
            var alreadyExists = await viewedEntriesQuery.AnyAsync();

            if (!alreadyExists)
            {
                await _viewedSiteInfos.InsertOneAsync(
                    new ViewedSiteInfo 
                        { 
                            SiteId = siteId, 
                            ViewedTimestamp = now 
                        }
                );
            }
        }       
    }
}