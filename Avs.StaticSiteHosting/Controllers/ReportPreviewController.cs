using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.Reporting;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportPreviewController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Index(ReportPreviewRequestModel previewModel, IReportDataFacade reportDataFacade)
        {
            var parameters = new ReportParameters()
            {
                SiteOwnerId = CurrentUserId
            };

            var filter = previewModel.Filter;
            if (filter is not null)
            {
                parameters.ApplyFilters(previewModel.Filter);
            }

            var reportModel = await reportDataFacade.GetReportDataAsync(parameters, previewModel.ReportType);

            return PartialView(reportModel);
        }
    }
}