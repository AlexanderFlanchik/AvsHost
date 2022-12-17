using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardOperationsController : BaseController
    {
        private readonly ISiteService _siteService;
        private readonly IUserService _userService;

        public DashboardOperationsController(ISiteService siteService, IUserService userService)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _userService = userService ?? throw new ArgumentException(nameof(userService));
        }

        [HttpPost]
        [Route("toggleSiteStatus")]
        public async Task<IActionResult> ToggleSiteStatus(ToggleSiteStatusRequestModel requestModel, IEventLogsService eventLogsService)
        {
            var siteId = requestModel.SiteId;
            var site = await _siteService.GetSiteByIdAsync(siteId);
            if (site == null)
            {
                return BadRequest($"No site with Id = {requestModel.SiteId} has been found.");
            }

            var siteName = site.Name;
            var currentUser = await _userService.GetUserByIdAsync(CurrentUserId);
            
            var currentUserName = currentUser.Name;
            var toggleResult = await _siteService.ToggleSiteStatusAsync(site);

            if (toggleResult)
            {
                await eventLogsService.InsertSiteEventAsync(siteId, "Site started", SiteEventType.Information,
                    $"Site '{siteName}' was started by {currentUserName}.");
            }
            else
            {
                await eventLogsService.InsertSiteEventAsync(siteId, "Site stopped", SiteEventType.Warning,
                   $"Site '{siteName}' was stopped by {currentUserName}.");
            }

            return Ok(toggleResult);
        }

        [HttpDelete("{siteId}")]
        public async Task<IActionResult> DeleteSite([Required] string siteId, [FromServices] IContentManager contentManager)
        {
            var siteToDelete = await _siteService.GetSiteByIdAsync(siteId);
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