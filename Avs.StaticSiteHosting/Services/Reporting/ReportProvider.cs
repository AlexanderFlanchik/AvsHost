using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Reporting;

public class ReportProvider : IReportProvider
{
    private readonly IReportDataFacade _reportDataFacade;
    private readonly IEnumerable<IReportRenderer> _reportRenderers;

    public ReportProvider(IReportDataFacade reportDataFacade, IEnumerable<IReportRenderer> reportRenderers)
    {
        _reportDataFacade = reportDataFacade;
        _reportRenderers = reportRenderers;
    }

    public async Task<byte[]> CreateReportAsync(ReportParameters parameters, ReportType reportType, ReportFormat format)
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

        var report = await _reportDataFacade.GetReportDataAsync(parameters, reportType);
        var reportContent = await renderer.RenderAsync(report);

        return reportContent;
    }
}