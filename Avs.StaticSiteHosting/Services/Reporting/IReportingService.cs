using Avs.StaticSiteHosting.Reports.Common;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Reporting
{
    public interface IReportingService
    {
        Task<byte[]> GenerateReportAsync(ReportParameters parameters, ReportType reportType, ReportFormat format);
    }
}