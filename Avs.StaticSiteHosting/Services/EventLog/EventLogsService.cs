﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;

namespace Avs.StaticSiteHosting.Web.Services.EventLog
{
    public interface IEventLogsService
    {
        Task<(long, IEnumerable<SiteEventModel>)> GetEventLogsAsync(EventLogsQuery query);
        Task<string> InsertSiteEventAsync(string siteId, string eventName, SiteEventType type, string details);
    }

    public class EventLogsService : IEventLogsService
    {
        private readonly IMongoCollection<SiteEvent> _events;

        public EventLogsService(MongoEntityRepository entityRepository)
        {
            _events = entityRepository.GetEntityCollection<SiteEvent>(GeneralConstants.SITE_EVENTS_COLLECTION);
        }

        public async Task<(long, IEnumerable<SiteEventModel>)> GetEventLogsAsync(EventLogsQuery query)
        {
            var queryFilter = new FilterDefinitionBuilder<SiteEvent>().Empty;
            if (!string.IsNullOrEmpty(query.CurrentUserId))
            {
                var sitesFilter = new FilterDefinitionBuilder<Site>().Eq(s => s.CreatedBy.Id, query.CurrentUserId);
                var userIdFilter = new FilterDefinitionBuilder<SiteEvent>().ElemMatch(se => se.Sites, sitesFilter);
                queryFilter &= userIdFilter;
            }

            if (query.DateFrom.HasValue)
            {
                queryFilter &= new FilterDefinitionBuilder<SiteEvent>().Gte(s => s.Timestamp, query.DateFrom.Value);
            }

            if (query.DateTo.HasValue)
            {
                queryFilter &= new FilterDefinitionBuilder<SiteEvent>().Lte(s => s.Timestamp, query.DateTo.Value);
            }

            if (query.Type.HasValue)
            {
                queryFilter &= new FilterDefinitionBuilder<SiteEvent>().Eq(s => s.Type, query.Type.Value);                
            }

            if (!string.IsNullOrEmpty(query.SiteId))
            {
                queryFilter &= new FilterDefinitionBuilder<SiteEvent>().Eq(s => s.SiteId, query.SiteId);
            }

            var aggr = _events.Aggregate()
                .Lookup<Site, SiteEvent>(GeneralConstants.SITES_COLLECTION, "SiteId", "_id", "Sites")
                .Match(queryFilter);

            var totalLogEvents = (await aggr.Count().FirstOrDefaultAsync())?.Count ?? 0;
            var events = await aggr.SortByDescending(t => t.Timestamp)                
                .Skip((query.Page - 1) * query.PageSize)
                .Limit(query.PageSize)
                .ToListAsync();

            var rows = events.Select(e => 
                new SiteEventModel 
                { 
                    EventId = e.Id, 
                    SiteId = e.Sites.FirstOrDefault()?.Id,
                    SiteName = e.Sites.FirstOrDefault()?.Name, 
                    Details = e.Details, 
                    Timestamp = e.Timestamp, 
                    Type = e.Type.ToString() 
                }).ToArray();
                        
            return (totalLogEvents, rows);
        }

        public async Task<string> InsertSiteEventAsync(string siteId, string eventName, SiteEventType type, string details)
        {
            var newEntry = new SiteEvent { SiteId = siteId, Type = type, Name = eventName, Timestamp = DateTime.UtcNow, Details = details };
            await _events.InsertOneAsync(newEntry);

            return newEntry.Id;
        }
    }
}