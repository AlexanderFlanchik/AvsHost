using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.Identity;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SiteSearchController : BaseController
    {
        [HttpGet("{siteNameTerm}")]
        public async Task<IActionResult> SearchSites(string siteNameTerm, ISiteService siteService, IUserService userService) 
            => Ok(await siteService.SearchSitesByName(
                siteNameTerm,
                await userService.IsAdminAsync(CurrentUserId) ? null : CurrentUserId
            ));
    }
}