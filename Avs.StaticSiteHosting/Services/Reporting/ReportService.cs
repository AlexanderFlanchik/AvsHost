using System.Threading.Tasks;
using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Contracts;

namespace Avs.StaticSiteHosting.Web.Services.Reporting
{
    public class ReportService : IReportingService
    {
        private readonly IReportProvider _reportProvider;

        public ReportService(IReportProvider reportProvider)
        {
            _reportProvider = reportProvider;
        }

        public Task<byte[]> GenerateReportAsync(ReportParameters parameters, ReportType reportType, ReportFormat format)
        {
            return _reportProvider.CreateReportAsync(parameters, reportType, format);
        }
    }
}