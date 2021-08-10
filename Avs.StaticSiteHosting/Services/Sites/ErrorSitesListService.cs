using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.Sites
{
    public interface IErrorSitesListService
    {
        Task<(long, IEnumerable<ErrorSiteModel>)> GetErrorSites(string ownerId, int page, int pageSize);
    }

    public class ErrorSitesListService : IErrorSitesListService
    {
        private readonly IMongoCollection<SiteEvent> _siteEvents;

        public ErrorSitesListService(MongoEntityRepository entityRepository)
        {
            _siteEvents = entityRepository.GetEntityCollection<SiteEvent>(GeneralConstants.SITE_EVENTS_COLLECTION);
        }

        public async Task<(long, IEnumerable<ErrorSiteModel>)> GetErrorSites(string ownerId, int page, int pageSize)
        {
            var sitesFilter = new FilterDefinitionBuilder<Site>().Eq(s => s.CreatedBy.Id, ownerId);
            var errorFilter = new FilterDefinitionBuilder<SiteEvent>().Eq(e => e.Type, SiteEventType.Error);
            var userIdFilter = new FilterDefinitionBuilder<SiteEvent>().ElemMatch(se => se.Sites, sitesFilter);

            var now = DateTime.UtcNow;
            var dateFrom = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var dateTo = dateFrom.AddHours(23).AddMinutes(59).AddSeconds(59);
            var dateFilter = new FilterDefinitionBuilder<SiteEvent>().Gte(d => d.Timestamp, dateFrom) &
                new FilterDefinitionBuilder<SiteEvent>().Lte(d => d.Timestamp, dateTo);

            var queryFilter = userIdFilter & errorFilter & dateFilter;
            
            var aggr =  _siteEvents.Aggregate()
             .Lookup<Site, SiteEvent>(GeneralConstants.SITES_COLLECTION, "SiteId", "_id", "Sites")
                .Match(queryFilter)
                .Group(g => g.SiteId, g => 
                                        new ErrorSiteModel 
                                            { 
                                                SiteId = g.Key, 
                                                SiteName = g.Max(s => s.Sites.First().Name), 
                                                Timestamp = g.Max(t => t.Timestamp) 
                                            }
                                        );

            var totalErrorEvents = (await aggr.Count().FirstOrDefaultAsync())?.Count ?? 0;
            var errorSitesList = await aggr.SortByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (totalErrorEvents, errorSitesList);            
        }
    }
}