using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceContentController : BaseController
    {
        private readonly IContentManager _contentManager;

        public ResourceContentController(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<IActionResult> GetResource(string siteId, string uploadSessionId, [Required] string contentExtension)
        {
            if (string.IsNullOrEmpty(siteId) && string.IsNullOrEmpty(uploadSessionId))
            {
                return BadRequest($"Either '{nameof(siteId)}' or '{uploadSessionId}' is required.");
            }

            var contentFiles = new List<ResourceContentSearchModel>();
            
            if (!string.IsNullOrEmpty(uploadSessionId))
            {
                var newUploadedFiles = _contentManager.GetNewUploadedFiles(uploadSessionId, contentExtension)
                    .Select(cn =>  new ResourceContentSearchModel(null, cn)
                    ).ToList();

                contentFiles.AddRange(newUploadedFiles);
            }

            var allContentItems = await _contentManager.GetUploadedContentAsync(siteId);
            var contentItems = allContentItems.Where(i => i.FileName.EndsWith(contentExtension))
                    .Select(i => 
                        new ResourceContentSearchModel(
                            i.Id, 
                            string.IsNullOrEmpty(i.DestinationPath) ? i.FileName : $"{i.DestinationPath}/{i.FileName}"
                         )
                    ).ToArray();

            contentFiles.AddRange(contentItems);

            return Ok(contentFiles);
        }
    }
}