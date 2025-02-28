using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Shared.Configuration;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentUploadController : ControllerBase
    {
        private readonly StaticSiteOptions staticSiteOptions;
        private readonly ILogger<ContentUploadController> _logger;
        private readonly IContentUploadService _contentUploadService;

        public ContentUploadController(IContentUploadService contentUploadService, IOptions<StaticSiteOptions> staticSiteOptions, ILogger<ContentUploadController> logger)
        {
            this.staticSiteOptions = staticSiteOptions.Value;
            _contentUploadService = contentUploadService;
            _logger = logger;
        }

        [Route("session")]
        public IActionResult StartUploadSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            _logger.LogInformation($"Started new upload session: ID = {sessionId}");
            
            Response.Headers.Append(GeneralConstants.UPLOAD_SESSION_ID, sessionId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadContent([Required] string uploadSessionId, string destinationPath, ContentUploadRequest uploadRequest)
        { 
            if (!_contentUploadService.ValidateDestinationPath(destinationPath))
            {
                return BadRequest("Invalid destination path. Network or relative paths are not allowed.");
            }
            
            var contentFile = uploadRequest.ContentFile;
            var contentFileName = contentFile.FileName;

            try
            {
                // Create folder and upload file
                await _contentUploadService.UploadContent(
                    uploadSessionId, 
                    contentFileName, 
                    destinationPath, 
                    contentFile.OpenReadStream(),
                    uploadRequest.CacheDuration
                );
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(destinationPath))
                {
                    return BadRequest("Invalid destination path. It must have a folder path format.");
                }

                _logger.LogError(ex.Message);
                
                return Problem($"Unable to upload {contentFileName}. Server error.");
            }

            _logger.LogInformation("Content file uploaded: {fileName}", contentFileName);
           
            return Ok();
        }

        [HttpPost]
        [Route("cancelupload")]
        public IActionResult CancelUpload([FromHeader(Name = GeneralConstants.UPLOAD_SESSION_ID)] string uploadSessionId)
        {
            var tempContentPath = staticSiteOptions.TempContentPath;
            string uploadFolderPath = Path.Combine(tempContentPath, uploadSessionId);
            
            if (!Directory.Exists(uploadFolderPath))
            {
                return NoContent();
            }

            Directory.Delete(uploadFolderPath, true);

            return NoContent();
        }
    }
}