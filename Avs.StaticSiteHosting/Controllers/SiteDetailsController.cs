using System;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class SiteDetailsController : BaseController
    {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;

        public SiteDetailsController(ISiteService siteService, IContentManager contentManager)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _contentManager = contentManager ?? throw new ArgumentException(nameof(contentManager));
        }

        [HttpGet]
        [Route("CheckSiteName")]
        public async Task<IActionResult> ValidateSiteName(string siteName, string siteId)
            => Json(!await _siteService.CheckSiteNameUsedAsync(siteName, siteId));

        [HttpGet("{siteId}")]
        public async Task<ActionResult<SiteDetailsResponse>> GetSiteDetails(string siteId)
        {
            var site = await _siteService.GetSiteByIdAsync(siteId);
            if (site == null)
            {
                return NotFound();
            }

            // User can edit only own site installations
            if (CurrentUserId != site.CreatedBy.Id)
            {
                return Unauthorized();
            }

            var uploadedFiles = await _contentManager.GetUploadedContentAsync(siteId);

            var siteDetailsResponse = new SiteDetailsResponse()
                {
                    SiteName = site.Name,
                    Description = site.Description,
                    IsActive = site.IsActive,
                    ResourceMappings = site.Mappings,
                    LandingPage = site.LandingPage,
                    Uploaded = uploadedFiles.ToList(),
                    TagIds = site.TagIds?.Select(x => x.Id).ToArray(),
                    DatabaseName = site.DatabaseName,
                };

            return siteDetailsResponse;
        }
    }
}