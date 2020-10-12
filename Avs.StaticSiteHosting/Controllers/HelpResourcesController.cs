using System;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpResourcesController : ControllerBase
    {
        private readonly IHelpContentService _helpContentService;

        public HelpResourcesController(IHelpContentService helpContentService)
        {
            _helpContentService = helpContentService ?? throw new ArgumentNullException(nameof(helpContentService));
        }

        [HttpGet("{resourceName}")]
        public async Task<IActionResult> GetHelpResource(string resourceName)
        {
            var resource = await _helpContentService.GetHelpResourceAsync(resourceName);
            
            return resource != null ? File(resource.Content, resource.ContentType) : (IActionResult)NotFound();
        }
    }
}