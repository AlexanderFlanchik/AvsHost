using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using Avs.StaticSiteHosting.Reports.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Reporting
{
    public class ReportDataFacade : IReportDataFacade
    {
        private readonly IEnumerable<IReportDataService> _reportDataServices;

        public ReportDataFacade(IEnumerable<IReportDataService> reportDataServices)
        {
            _reportDataServices = reportDataServices;
        }

        public async Task<Report> GetReportDataAsync(ReportParameters parameters, ReportType reportType)
        {
            var dataService = _reportDataServices.FirstOrDefault(ds => ds.Type == reportType);
            if (dataService is null)
            {
                throw new InvalidOperationException($"No data provider for '{reportType}' report.");
            }

            var reportData = await dataService.GetReportDataAsync(reportType, parameters);

            return reportData;
        }
    }
}