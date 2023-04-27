using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Reporting.SiteErrors
{
    public class SiteErrorsStatisticsReportDataService : IReportDataService
    {
        private readonly IMongoCollection<SiteEvent> _events;

        public SiteErrorsStatisticsReportDataService(MongoEntityRepository entityRepository)
        {
            _events = entityRepository.GetEntityCollection<SiteEvent>(GeneralConstants.SITE_EVENTS_COLLECTION);
        }

        public ReportType Type => ReportType.ErrorStatistics;

        public async Task<Report> GetReportDataAsync(ReportType reportType, ReportParameters reportParameters)
        {
            var sitesFilter = new FilterDefinitionBuilder<Site>().Where(s => s.CreatedBy.Id == reportParameters.SiteOwnerId);
            var siteIdsValue = (JArray)reportParameters["siteIds"];
            var siteIdsSelected = siteIdsValue.Values<string>().ToArray();
            
            if (siteIdsSelected.Any())
            {
                sitesFilter &= new FilterDefinitionBuilder<Site>().In(s => s.Id, siteIdsSelected);
            }

            DateTime dateFrom, dateTo;
            if (!DateTime.TryParse(reportParameters["dateFrom"]?.ToString(), out dateFrom) ||
                !DateTime.TryParse(reportParameters["dateTo"]?.ToString(), out dateTo))
            {
                throw new InvalidOperationException("DateFrom & DateTo fields must not be empty.");
            }

            dateTo = dateTo.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
            var errorsOnlyFilter = new FilterDefinitionBuilder<SiteEvent>().Where(e => e.Type == SiteEventType.Error);
            var datesFilter = new FilterDefinitionBuilder<SiteEvent>().Gte(e => e.Timestamp, dateFrom) & 
                new FilterDefinitionBuilder<SiteEvent>().Lte(e => e.Timestamp, dateTo);

            var eventsFilter = errorsOnlyFilter & datesFilter & new FilterDefinitionBuilder<SiteEvent>().ElemMatch(e => e.Sites, sitesFilter);

            var query = _events.Aggregate()
                  .Lookup<Site, SiteEvent>(GeneralConstants.SITES_COLLECTION, "SiteId", "_id", "Sites")
                  .Match(eventsFilter)
                  .Sort(new SortDefinitionBuilder<SiteEvent>().Descending(e =>e.Timestamp));

            var events = await query.ToListAsync();
            var report = new Report() { Title = "Site Errors Statistics" };
            var datesLine = $"Site errors from {dateFrom.ToShortDateString()} to {dateTo.ToShortDateString()}";

            report.Sections.Add(new TextSection() { Content = datesLine });
            var reportRows = new List<TableRow>();
            
            foreach (var ev in events)
            {
                var siteName = ev.Sites.First().Name;
                reportRows.Add(
                    new TableRow()
                    {
                        Cells = new[]
                        {
                            new TableCell { Value = siteName },
                            new TableCell { Value = ev.Timestamp.ToString() },
                            new TableCell { Value = ev.Details } 
                        }
                    });
            }

            report.Sections.Add(
                    new TableSection(
                            new[] { "Site Name", "Timestamp (UTC)", "Error Details" }, 
                            reportRows.ToArray()
                    ));

            return report;
        }
    }
}