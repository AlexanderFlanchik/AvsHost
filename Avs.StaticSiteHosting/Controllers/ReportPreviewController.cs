using Avs.StaticSiteHosting.Reports.Common;
using Avs.StaticSiteHosting.Reports.Models;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.Reporting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportPreviewController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> Index(ReportPreviewRequestModel request, IReportDataFacade reportDataFacade, ILogger<ReportController> logger)
        {
            var parameters = new ReportParameters()
            {
                SiteOwnerId = CurrentUserId
            };

            var filter = request.Filter;
            if (filter is not null)
            {
                parameters.ApplyFilters(request.Filter);
            }

            Report reportModel = null;
            ReportPreviewErrorModel errorModel = null;
            
            try
            {
                reportModel = await reportDataFacade.GetReportDataAsync(parameters, request.ReportType);
            }
            catch (NoRequiredFitlerException ex)
            {
                errorModel = new ReportPreviewErrorModel()
                {
                    RequiredFilterMissing = ex.FilterNames
                };
            }
            catch (ReportPreviewException ex)
            {
                logger.LogError(ex, "Unable to generate a report preview for type '{type}' and site owner ID = '{userId}'.", 
                    request.ReportType.ToString(), CurrentUserId);
                
                errorModel = new ReportPreviewErrorModel() { Error = ex.Message };
            }

            if (errorModel is not null)
            {
                return PartialView("ReportPreviewError", errorModel);
            }

            Response.Headers.Append(GeneralConstants.REPORT_PREVIEW_READY, new StringValues(bool.TrueString));

            return PartialView(reportModel);
        }
    }
}