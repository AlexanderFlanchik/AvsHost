using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class ResourcePreviewContentController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly StaticSiteOptions options;
        private readonly ISiteService _siteService;
        private readonly ILogger<ResourcePreviewContentController> _logger;

        public ResourcePreviewContentController(IContentManager contentManager, IOptions<StaticSiteOptions> staticSiteOptions, ISiteService siteService,
            ILogger<ResourcePreviewContentController> logger)
        {
            _contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager));
            options = staticSiteOptions.Value;
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async ValueTask<IActionResult> PreviewResourceContent(string uploadSessionId, string siteId, [Required] string contentPath)
        {
            if (string.IsNullOrEmpty(uploadSessionId) && string.IsNullOrEmpty(siteId))
            {
                return BadRequest();
            }

            string resourcePath;
            contentPath = HttpUtility.UrlDecode(contentPath);

            if (!string.IsNullOrEmpty(uploadSessionId))
            {
                var temporaryDirectory = Path.Combine(options.TempContentPath, uploadSessionId);
                resourcePath = contentPath.Replace($"/{GeneralConstants.NEW_RESOURCE_PATTERN}", temporaryDirectory);
            }
            else
            {
                var site = await _siteService.GetSiteByIdAsync(siteId);
                if (site is null)
                {
                    return BadRequest($"No site with Id = {siteId} has been found. Please make sure you send a correct site Id.");
                }

                var siteFolder = Path.Combine(options.ContentPath, site.Name);
                resourcePath = contentPath.Replace($"/{GeneralConstants.EXIST_RESOURCE_PATTERN}", siteFolder);
            }

            resourcePath = resourcePath.Replace('/', '\\');
            
            var fileInfo = new FileInfo(resourcePath);
            if (!fileInfo.Exists)
            {
                _logger.LogWarning($"Unable to get a resource for preview. No content with path '{resourcePath}' found.");

                return NotFound();
            }

            var contentType = _contentManager.GetContentType(resourcePath);
            var contentStream = fileInfo.OpenRead();

            return File(contentStream, contentType);
        }
    }
}
