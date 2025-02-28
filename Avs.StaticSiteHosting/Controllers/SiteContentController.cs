using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Avs.StaticSiteHosting.Web.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class SiteContentController(IContentManager contentManager) : BaseController
    {
        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> GetContent([Required] string contentItemId, int? maxWidth, [FromServices] ImageResizeService resizeService)
        {
            var (fileName, contentType, contentStream) = await contentManager.GetContentFileAsync(contentItemId);
            contentType ??= "application/octet-stream";

            if (contentStream is null)
            {
                return NotFound();
            }

            Response.Headers.Append("content-disposition", $"attachment;filename={fileName}");

            if (maxWidth.HasValue) // its graphic content, possible we need to resize it to fit max width.
            {
                try
                {
                    var resizedImageStream = await resizeService.GetResizedImageStreamAsync(contentStream, contentType, maxWidth.Value);

                    return File(resizedImageStream, contentType);
                }
                catch (InvalidOperationException)
                {
                    return BadRequest(ModelState); // the image is invalid or file is not image
                }
                finally
                {
                    contentStream.Dispose();
                }
            }

            return File(contentStream, contentType);
        }

        [HttpPut]
        [Route("content-edit/{contentItemId}")]
        public async Task<IActionResult> EditContent([Required] string contentItemId, EditContentModel model)
        {
            await contentManager.UpdateContentItem(contentItemId, model.Content, model.CacheDuration);

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteContent(string contentItemId, string contentItemName, string uploadSessionId)
        {
            if (!string.IsNullOrEmpty(contentItemName) && string.IsNullOrEmpty(uploadSessionId))
            {
                return BadRequest("Content item ID must be specified with non-empty upload session ID.");
            }

            bool isDeleted;
            string errorMsg;

            if (!string.IsNullOrEmpty(contentItemId))
            {
                isDeleted = await contentManager.DeleteContentByIdAsync(contentItemId);

                errorMsg = isDeleted ? string.Empty : $"Cannot delete content with ID: {contentItemId}";
            }
            else if (!string.IsNullOrEmpty(contentItemName))
            {
                isDeleted = contentManager.DeleteNewUploadedFile(contentItemName, uploadSessionId);

                errorMsg = isDeleted ? string.Empty : $"No file deleted, content file name {contentItemName}, session ID: {uploadSessionId}.";
            }
            else
            {
                return BadRequest("You must specify either content item ID or name with upload session ID");
            }

            return isDeleted ? Ok() : BadRequest(errorMsg);
        }
    }
}