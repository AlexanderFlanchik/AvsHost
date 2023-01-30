using Avs.StaticSiteHosting.Reports.Common;

namespace Avs.StaticSiteHosting.Reports.Contracts
{
    public interface IReportProvider
    {
        /// <summary>
        /// Report type <see cref="ReportType"/>
        /// </summary>
        ReportType Type { get; }

        /// <summary>
        /// Produces the report specified by type and parameters
        /// </summary>
        /// <param name="parameters">Report parameters</param>
        /// <param name="format">Format (.PDF or .XLSX)</param>
        /// <returns>Task with report content</returns>
        Task<byte[]> CreateReportAsync(ReportParameters parameters, ReportFormat format);
    }
}