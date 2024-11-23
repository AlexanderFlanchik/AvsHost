using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Web.Services.Reporting;
using Avs.StaticSiteHosting.Web.Common;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : BaseController
    {
        private readonly IReportingService _reportingService;

        public ReportController(IReportingService reportingService)
        {
            _reportingService = reportingService;
        }

        [HttpPost]
        [Route("{reportType}/{reportFormat}")]
        public async Task<IActionResult> Index(ReportType reportType, ReportFormat reportFormat, 
                    [ModelBinder(typeof(ReportParametersBinder))] ReportParameters reportParameters)
        {
            var reportContent = await _reportingService.GenerateReportAsync(reportParameters, reportType, reportFormat);

            var (extension, contentType) = reportFormat == ReportFormat.PDF ?
                ("pdf", "application/pdf") :
                ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            var reportFileName = $"{reportType}.{extension}";

            return File(reportContent, contentType, reportFileName);
        }
    }
}