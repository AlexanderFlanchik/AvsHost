using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Models;

namespace Avs.StaticSiteHosting.Reports.Contracts
{
    public interface IReportRenderer
    {
        /// <summary>
        /// Report format which current implementation supports.
        /// </summary>
        ReportFormat Format { get; }

        /// <summary>
        /// Generates report and returns a byte array with its content
        /// </summary>
        /// <param name="report">Report data</param>
        /// <returns>Task with array of bytes.</returns>
        Task<byte[]> RenderAsync(Report report);
    }
}