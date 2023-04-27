using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Models;

namespace Avs.StaticSiteHosting.Reports.Contracts
{
    /// <summary>
    /// Produces data for a report
    /// </summary>
    public interface IReportDataService
    {
        /// <summary>
        /// Report type <see cref="ReportType"/>
        /// </summary>
        ReportType Type { get; }

        /// <summary>
        /// Returns data for a report as an instance of <see cref="Report"/>
        /// </summary>
        /// <param name="reportType">Report type</param>
        /// <param name="reportParameters">Report parameters</param>
        /// <returns>a task with filled report instance.</returns>
        Task<Report> GetReportDataAsync(ReportType reportType, ReportParameters reportParameters);
    }
}