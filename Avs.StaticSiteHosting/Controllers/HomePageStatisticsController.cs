using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Avs.StaticSiteHosting.Web.Services.Sites;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomePageStatisticsController : BaseController
    {
        [HttpGet]
        [Route("last-visits")]
        public async Task<IActionResult> GetLastSiteVists([FromServices] ISiteStatisticsService siteStatisticsService)
            => Ok(await siteStatisticsService.GetLatestSiteVisits(CurrentUserId));

        [HttpGet]
        [Route("error-sites")]
        public async Task<IActionResult> GetErrorSites(int page, int pageSize, [FromServices] IErrorSitesListService errorSitesListService)
        {
            var (totalErrorSites, errorSites) = await errorSitesListService.GetErrorSites(CurrentUserId, page, pageSize);
            Response.Headers.Add(GeneralConstants.TOTAL_ROWS_AMOUNT, new StringValues(totalErrorSites.ToString()));

            return Ok(errorSites);
        }      
    }
}