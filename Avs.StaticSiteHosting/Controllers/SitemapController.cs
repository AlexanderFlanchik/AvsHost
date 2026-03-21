using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SitemapController : BaseController
{
    [HttpPost]
    [Route("generate/{siteId}")]
    public async Task<IActionResult> Generate(string siteId, ISitemapManager sitemapManager)
    {
        var sitemapResult = await sitemapManager.CreateOrUpdateSitemapAsync(siteId);
        
        return string.IsNullOrEmpty(sitemapResult.Error) switch
        {
            false => BadRequest(new { sitemapResult.Error }),
            _ => Ok(new { sitemapResult.SitemapId, sitemapResult.SizeKb })
        };
    }
}