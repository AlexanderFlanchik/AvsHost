using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Services;
using Avs.StaticSiteHosting.Services.ContentManagement;
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
            bool toggleResult;
            try
            {
                toggleResult = await _siteService.ToggleSiteStatusAsync(requestModel.SiteId)
                    .ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(toggleResult);
        }

        [HttpDelete("{siteId}")]
        public async Task<IActionResult> DeleteStite([Required] string siteId, [FromServices] IContentManager contentManager)
        {
            var siteToDelete = await _siteService.GetSiteByIdAsync(siteId).ConfigureAwait(false);
            if (siteToDelete == null)
            {
                return NotFound();
            }

            await contentManager.DeleteSiteContentAsync(siteToDelete);
            await _siteService.DeleteSiteAsync(siteId);

            return Ok();
        }
    }
}