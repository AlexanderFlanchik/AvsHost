using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.EventLog;

namespace Avs.StaticSiteHosting.Web.Services.Reporting.SiteEvents
{
    public class SiteEventsReportDataService : IReportDataService
    {
        private readonly IEventLogsService _eventLogService;
        private readonly ISiteService _siteService;

        public SiteEventsReportDataService(IEventLogsService eventLogService, ISiteService siteService)
        {
            _eventLogService = eventLogService;
            _siteService = siteService;
        }

        public ReportType Type => ReportType.SiteEvents;

        public async Task<Report> GetReportDataAsync(ReportType reportType, ReportParameters reportParameters)
        {
            string siteId = reportParameters["siteId"]?.ToString();
            
            if (string.IsNullOrEmpty(siteId))
            {
                throw new NoRequiredFitlerException(["Site"]);
            }

            var site = await _siteService.GetSiteByIdAsync(siteId);
            if (site is null)
            {
                throw new ReportPreviewException($"The site with ID = '{siteId}' was not found.");
            }
 
            var eventLogsQuery = new EventLogsQuery()
            {
                SiteId = siteId,
                CurrentUserId = reportParameters.SiteOwnerId,
            };

            var (_, eventLogs) = await _eventLogService.GetEventLogsAsync(eventLogsQuery);
            var report = new Report() { Title = "Site Events report" };
            report.Sections.Add(new TextSection { Content = $"Site: {site.Name}" });

            var logRows = new List<TableRow>();
            foreach (var log in eventLogs)
            {
                var row = new TableRow() 
                { 
                    Cells = new[] { new TableCell { Value = log.Timestamp }, new TableCell { Value = log.Type }, new TableCell { Value = log.Details } } 
                };

                logRows.Add(row);
            }

            report.Sections.Add(
                    new TableSection(new[] { "Timestamp", "Event Type", "Details" }, logRows.ToArray())
            );

            return report;
        }
    }
}