using Avs.StaticSiteHosting.Reports.Common;

namespace Avs.StaticSiteHosting.Reports.Contracts
{
    /// <summary>
    /// Report provider service which produces a report with type <see cref="ReportType"/>
    /// </summary>
    public interface IReportProvider
    {
        /// <summary>
        /// Produces the report specified by type and parameters
        /// </summary>
        /// <param name="parameters">Report parameters</param>
        /// <param name="format">Format (.PDF or .XLSX)</param>
        /// <returns>Task with report content</returns>
        Task<byte[]> CreateReportAsync(ReportParameters parameters, ReportType reportType, ReportFormat format);
    }
}