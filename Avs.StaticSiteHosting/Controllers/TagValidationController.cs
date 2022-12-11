using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Avs.StaticSiteHosting.Web.Services;

namespace Avs.StaticSiteHosting.Web.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class TagValidationController : BaseController
{
    [HttpGet]
    [Route("check-new-tag")]
    public async Task<IActionResult> NewTagExists(string tagName, ITagsService tagsService)
        => Json(await tagsService.TagExists(CurrentUserId, tagName));

    [HttpGet]
    [Route("check-tag-use")]
    public async Task<IActionResult> IsTagUsed(string tagId, ITagSiteService siteService)
        => Json(await siteService.IsTagUsedInSites(tagId));
}