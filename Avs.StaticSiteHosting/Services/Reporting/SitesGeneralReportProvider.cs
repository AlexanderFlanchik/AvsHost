using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Site = Avs.StaticSiteHosting.Web.Models.Site;

namespace Avs.StaticSiteHosting.Web.Services.Reporting
{
    public class SitesGeneralReportProvider : IReportProvider
    {
        private readonly IMongoCollection<Site> _sites;
        private readonly IEnumerable<IReportRenderer> _reportRenderers;

        public SitesGeneralReportProvider(MongoEntityRepository entityRepository, IEnumerable<IReportRenderer> reportRenderers)
        {
            _sites = entityRepository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);
            _reportRenderers = reportRenderers;
        }

        public ReportType Type => ReportType.SitesGeneral;

        public async Task<byte[]> CreateReportAsync(ReportParameters parameters, ReportFormat format)
        {
            if (string.IsNullOrEmpty(parameters.SiteOwnerId))
            {
                throw new InvalidOperationException("Site owner ID must be specified.");
            }

            var renderer = _reportRenderers.Where(r => r.Format == format).FirstOrDefault();
            if (renderer is null)
            {
                throw new InvalidOperationException($"No renderer for {format} format found.");
            }

            var query = _sites.Aggregate()
                .Match(new FilterDefinitionBuilder<Site>().Where(s => s.CreatedBy.Id == parameters.SiteOwnerId))
                .Lookup<ContentItem, Site>(GeneralConstants.CONTENT_ITEMS_COLLECTION, "_id", "Site._id", "ContentItems");
    
            var sitesList = await query.ToListAsync();

            var reportData = sitesList.Select(s => 
                new SitesGeneralReportRow 
                { 
                    SiteId = s.Id,
                    SiteName = s.Name,
                    CreatedAt = s.LaunchedOn,
                    Status = s.IsActive ? "Running" : "Stopped",
                    LastStopped = s.LastStopped,
                    ContentFiles = s.ContentItems.Count,
                    StorageUsedKb = s.ContentItems.Sum(i => i.Size)
                }).ToList();

            var report = new Report() { Title = "Site General Information" };

            report.Sections.Add(
                new TableSection(new[] { "Site Name", "Created At", "Status", "Last Stopped", "Content Files", "Storage Used, Kb" },
                reportData.Select(
                    r => new TableRow
                            { 
                                Cells = new object[] { r.SiteName, r.CreatedAt, r.Status, r.LastStopped, r.ContentFiles, r.StorageUsedKb } 
                            }
                ).ToArray())
            );

            var reportContent = await renderer.RenderAsync(report);

            return reportContent;
        }

        private class SitesGeneralReportRow
        {
            public string SiteId { get; set; }
            public string SiteName { get; set; }
            public DateTime? CreatedAt { get; set; }
            public string Status { get; set; }
            public DateTime? LastStopped { get; set; }
            public int ContentFiles { get; set; }
            public decimal StorageUsedKb { get; set; }
        }
    }
}