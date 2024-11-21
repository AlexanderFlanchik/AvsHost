using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Models.SiteStatistics;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Reporting;

public class SiteVisitsStatisticsReportDataService : IReportDataService
{
    private readonly IMongoCollection<ViewedSiteInfo> _visits;
    private readonly IMongoCollection<Site> _sites;

    public SiteVisitsStatisticsReportDataService(MongoEntityRepository entityRepository)
    {
        _visits = entityRepository.GetEntityCollection<ViewedSiteInfo>(GeneralConstants.SITE_VIEWED_INFO_COLLECTION);
        _sites = entityRepository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);
    }

    public ReportType Type => ReportType.VisitStatistics;

    public async Task<Report> GetReportDataAsync(ReportType reportType, ReportParameters reportParameters)
    {
        DateTime dateFrom, dateTo;
        if (!DateTime.TryParse(reportParameters["dateFrom"]?.ToString(), out dateFrom) ||
            !DateTime.TryParse(reportParameters["dateTo"]?.ToString(), out dateTo))
        {
            throw new InvalidOperationException("DateFrom & DateTo fields must not be empty.");
        }

        dateTo = dateTo.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

        var dateFilter = new FilterDefinitionBuilder<ViewedSiteInfo>().Gte(v => v.ViewedTimestamp, dateFrom)
            & new FilterDefinitionBuilder<ViewedSiteInfo>().Lte(v => v.ViewedTimestamp, dateTo);

        bool siteIdsSelected = false;
        var siteIdsValue = (JArray)reportParameters["siteIds"];
        var siteIds = siteIdsValue.Values<string>().ToArray();
        var siteNames = Array.Empty<string>();
        
        if (!siteIds.Any())
        {
            var ownerSiteFilter = new FilterDefinitionBuilder<Site>().Where(s => s.CreatedBy.Id == reportParameters.SiteOwnerId);
            var siteIdAndNameProjection = new ProjectionDefinitionBuilder<Site>().Expression(i => new { i.Id, i.Name });
            var sites = await _sites.Find(ownerSiteFilter).Project(siteIdAndNameProjection).ToListAsync();
            
            siteIds = sites.Select(s => s.Id).ToArray();
            siteNames = sites.Select(s => s.Name).ToArray();
        }
        else
        {
            var siteIdsFilter = new FilterDefinitionBuilder<Site>().Where(s => siteIds.Contains(s.Id));
            var siteNameProjection = new ProjectionDefinitionBuilder<Site>().Expression(i => i.Name);
            siteNames = (await _sites.Find(siteIdsFilter).Project(siteNameProjection).ToListAsync())
                .ToArray();
            
            siteIdsSelected = true;
        }

        var sIdsFilter = new FilterDefinitionBuilder<ViewedSiteInfo>().In(s => s.SiteId, siteIds);

        var visits = await _visits.Aggregate()
                .Match(sIdsFilter & dateFilter)
                .Lookup<Site, ViewedSiteInfo>(GeneralConstants.SITES_COLLECTION, "SiteId", "_id", "Sites")
                .Unwind(v => v.Sites)
                .Project(
                    o => new
                    {
                        Id = o["SiteId"],
                        SiteName = o["Sites"]["Name"],
                        LaunchedOn = o["Sites"]["LaunchedOn"]
                    }
                ).Group(k => k.Id,
                    g => new
                    {
                        SiteId = g.Key,
                        SiteName = g.Max(i => i.SiteName),
                        LaunchedOn = g.Max(i => i.LaunchedOn),
                        Visits = g.Count()
                    }
                ).ToListAsync();

        var report = new Report() { Title = $"Site visits statistics" };

        var siteNamesList = "All";
        if (siteIdsSelected)
        {
            siteNamesList = string.Join(", ", siteNames.Distinct());
        }

        report.Sections.Add(new TextSection { Content = $"Sites: {siteNamesList}" });
        report.Sections.Add(new TextSection { Content = $"Visits from {dateFrom.ToShortDateString()} to {dateTo.ToShortDateString()}"});
        
        var rows = new List<TableRow>();
        var totalVisits = 0;

        foreach (var visit in visits)
        {
            totalVisits += visit.Visits;

            var row = new TableRow();
            var visitDate = visit.LaunchedOn.ToUniversalTime();
            row.Cells = new TableCell[] { visit.SiteName.ToString(), visitDate.ToString(), visit.Visits.ToString() };
            rows.Add(row);
        }

        var totalRow = new TableRow() { Cells = new TableCell[3] };
        TableCell totalNameCell = "Total:";
        TableCell totalVisitsCell = totalVisits;

        totalRow.Cells[1] = totalNameCell.Bold().WithAlign(TableCellAlign.Right);
        totalRow.Cells[2] = totalVisitsCell.Bold();
     
        var tableSection = new TableSection(new[] { "Site Name", "Launched On (UTC)", "Visits Amount" }, rows.ToArray());
        tableSection.Totals = totalRow;

        report.Sections.Add(tableSection);

        return report;
    }
}