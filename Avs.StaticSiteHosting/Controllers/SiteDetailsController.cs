using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.Sites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class SiteDetailsController(ISiteDetailsService siteDetailsService) : BaseController
    {
        [HttpGet]
        [Route("CheckSiteName")]
        public async Task<IActionResult> ValidateSiteName(string siteName, string siteId)
            => Json(!await siteDetailsService.CheckSiteNameUsedAsync(siteName, siteId));

        [HttpGet("{siteId}")]
        public async Task<ActionResult<SiteDetailsResponse>> GetSiteDetails(string siteId)
        {
            var (error, response) = await siteDetailsService.GetSiteDetailsAsync(siteId, CurrentUserId);

            return (error, response) switch
            {
                (NotFoundException, null) => NotFound(),
                (UnauthorizedException, null) => Unauthorized(),
                (null, {} details) => Ok(details),
                _ => BadRequest()
            };
        }
    }
}