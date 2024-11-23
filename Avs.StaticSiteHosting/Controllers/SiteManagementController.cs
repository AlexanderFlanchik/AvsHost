using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SiteManagementController(ISiteManagementService siteManagementService) : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> CreateSite(SiteDetailsModel siteDetails)
        {
            var (response, ex) = await siteManagementService.CreateSiteAndProcessContentAsync(siteDetails, CurrentUserId);

            return (response, ex) switch
            {
                (null, ConflictException conflictException) => Conflict(conflictException.Message),
                (null, BadRequestException badRequestException) => BadRequest(badRequestException.Message),
                ({ } result, null) => Created($"/sitedetails/{result.SiteId}", result),
                _ => BadRequest()
            };
        }

        [HttpPut("{siteId}")]
        public async Task<IActionResult> UpdateSite(string siteId, SiteDetailsModel siteDetails)
        {
            var (response, ex) = await siteManagementService.UpdateSiteAndProcessContent(siteId, CurrentUserId, siteDetails);
            
            return (response, ex) switch
            {
                (null, NotFoundException) => NotFound(),
                (null, ConflictException exception) => Conflict(exception.Message),
                (null, UnauthorizedAccessException) => Unauthorized(),
                ({ } result, null) => Ok(result),
                _ => BadRequest()
            };
        }
    }
}