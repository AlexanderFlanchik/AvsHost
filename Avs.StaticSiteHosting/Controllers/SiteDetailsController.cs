using System;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using Avs.StaticSiteHosting.Services;
using Avs.StaticSiteHosting.Services.ContentManagement;
using Avs.StaticSiteHosting.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteDetailsController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        
        public SiteDetailsController(ISiteService siteService, IContentManager contentManager)
        {
            _siteService = siteService 
                    ?? throw new ArgumentNullException(nameof(siteService));
            _contentManager = contentManager
                    ?? throw new ArgumentException(nameof(contentManager));
        }

        [HttpGet]
        [Route("CheckSiteName")]
        public async Task<IActionResult> ValidateSiteName(string siteName, string siteId)
            => Json(!await _siteService.CheckSiteNameUsedAsync(siteName, siteId));

        [HttpGet("{siteId}")]
        public async Task<ActionResult<SiteDetailsResponse>> GetSiteDetails(string siteId, [FromServices] IUserService userService)
        {
            var site = await _siteService.GetSiteByIdAsync(siteId);
            if (site == null)
            {
                return NotFound();
            }

            var userId = User.Claims.FirstOrDefault(r => r.Type == AuthSettings.UserIdClaim)?.Value;
            var currentUser = await userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (currentUser == null)
            {
                return BadRequest("Cannot find user by ID provided.");
            }

            // User can edit only own site installations
            if (userId != site.CreatedBy.Id)
            {
                return Unauthorized();
            }

            var uploadedFiles = await _contentManager.GetUploadedContentAsync(siteId).ConfigureAwait(false);

            var siteDetailsResponse = new SiteDetailsResponse()
                {
                    SiteName = site.Name,
                    Description = site.Description,
                    IsActive = site.IsActive,
                    ResourceMappings = site.Mappings,
                    Uploaded = uploadedFiles.ToList()
                };

            return siteDetailsResponse;
        }
                       
        [HttpPost]
        public async Task<IActionResult> CreateSite(SiteDetailsModel siteDetails, [FromServices] IUserService userService)
        {
            var uploadSessionId = siteDetails.UploadSessionId;
            if (string.IsNullOrEmpty(uploadSessionId))
            {
                return BadRequest("No uploaded files found.");
            }

            if (await _siteService.CheckSiteNameUsedAsync(siteDetails.SiteName, null))
            {
                return Conflict("This site name is already in use.");
            }

            var userId = User.Claims.FirstOrDefault(r => r.Type == AuthSettings.UserIdClaim)?.Value;
            var currentUser = await userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (currentUser == null)
            {
                return BadRequest("Cannot find user by ID provided.");
            }

            var newSite = await _siteService.CreateSiteAsync(new Site()
                {
                    Name = siteDetails.SiteName,
                    Description = siteDetails.Description,
                    IsActive = siteDetails.IsActive,
                    CreatedBy = currentUser,
                    LaunchedOn = DateTime.UtcNow,
                    Mappings = siteDetails.ResourceMappings
                }).ConfigureAwait(false);

            await _contentManager.CreateSiteContentAsync(newSite, uploadSessionId)
                .ConfigureAwait(false);

            return Created(
                    $"api/siteDetails?siteId={newSite.Id}", 
                    newSite);
        }

        [HttpPut("{siteId}")]
        public async Task<IActionResult> UpdateSite(string siteId, SiteDetailsModel siteDetails, [FromServices] IUserService userService)
        {
            var siteToUpdate = await _siteService.GetSiteByIdAsync(siteId).ConfigureAwait(false);
            if (siteToUpdate == null)
            {
                return NotFound();
            }

            if (await _siteService.CheckSiteNameUsedAsync(siteDetails.SiteName, siteId))
            {
                return Conflict("This site name is already in use.");
            }

            var userId = User.Claims.FirstOrDefault(r => r.Type == AuthSettings.UserIdClaim)?.Value;
            var currentUser = await userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (currentUser == null)
            {
                return BadRequest("Cannot find user by ID provided.");
            }
                      
            if (siteToUpdate.CreatedBy.Id != userId)
            {
                return Unauthorized();
            }

            siteToUpdate.Name = siteDetails.SiteName;
            siteToUpdate.IsActive = siteDetails.IsActive;
            siteToUpdate.Description = siteDetails.Description;
            siteToUpdate.Mappings = siteDetails.ResourceMappings;

            await _siteService.UpdateSiteAsync(siteToUpdate).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(siteDetails.UploadSessionId))
            {
                await _contentManager.CreateSiteContentAsync(siteToUpdate, siteDetails.UploadSessionId).ConfigureAwait(false);
            }
            
            return NoContent();
        }
    }
}