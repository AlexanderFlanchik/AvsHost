using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Models;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Reporting
{
    /// <summary>
    /// Represents data source provider for report generation
    /// </summary>
    public interface IReportDataFacade
    {
        /// <summary>
        /// Gets data for the report with the type given and parameters.
        /// </summary>
        /// <param name="parameters">Report parameters</param>
        /// <param name="reportType">Report type</param>
        /// <returns></returns>
        Task<Report> GetReportDataAsync(ReportParameters parameters, ReportType reportType);
    }
}