using System;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardOperationsController : ControllerBase
    {
        private readonly ISiteService _siteService;
        
        public DashboardOperationsController(ISiteService siteService)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }

        [HttpPost]
        [Route("toggleSiteStatus")]
        public async Task<IActionResult> ToggleSiteStatus(ToggleSiteStatusRequestModel requestModel)
        {
            var toggleResult = await _siteService.ToggleSiteStatusAsync(requestModel.SiteId)
                .ConfigureAwait(false);

            return Ok(toggleResult);
        }
    }
}