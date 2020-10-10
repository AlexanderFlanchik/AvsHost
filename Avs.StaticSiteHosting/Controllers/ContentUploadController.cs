using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentUploadController : ControllerBase
    {
        private readonly StaticSiteOptions staticSiteOptions;
        private readonly ILogger<ContentUploadController> _logger;
        public ContentUploadController(IOptions<StaticSiteOptions> staticSiteOptions, ILogger<ContentUploadController> logger)
        {
            this.staticSiteOptions = staticSiteOptions.Value;
            _logger = logger;
        }

        [Route("session")]
        public IActionResult StartUploadSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            _logger.LogInformation($"Started new upload session: ID = {sessionId}");
            
            Response.Headers.Add(GeneralConstants.UPLOAD_SESSION_ID, sessionId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadContent([Required] string uploadSessionId, string destinationPath, [Required] IFormFile contentFile)
        { 
            var tempContentPath = staticSiteOptions.TempContentPath;
            string uploadFolderPath = Path.Combine(tempContentPath, uploadSessionId);

            if (!string.IsNullOrEmpty(destinationPath))
            {
                if (destinationPath.StartsWith('\\') || destinationPath.StartsWith('/'))
                {
                    return BadRequest("Invalid destination path. Network or relative paths are not allowed.");
                }

                uploadFolderPath = Path.Combine(uploadFolderPath, destinationPath);
            }

            try
            {
                Directory.CreateDirectory(uploadFolderPath);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(destinationPath))
                {
                    return BadRequest("Invalid destination path. It must have a folder path format.");
                }

                _logger.LogError(ex.Message);
                return Problem($"Unable to upload {contentFile.FileName}. Server error.");
            }

            try
            {
                var newFilepath = Path.Combine(uploadFolderPath, contentFile.FileName);
                var fi = new FileInfo(newFilepath);

                using var fiStream = fi.OpenWrite();                
                await contentFile.CopyToAsync(fiStream).ConfigureAwait(false);                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem($"Unable to upload {contentFile.FileName}. Server error.");
            }

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

            void ClearFolder(string path)
            {
                var entries = Directory.EnumerateFileSystemEntries(path);
                foreach (var entry in entries)
                {
                    if ((System.IO.File.GetAttributes(entry) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        ClearFolder(entry);
                    }
                    else
                    {
                        new FileInfo(entry).Delete();
                    }    
                }

                Directory.Delete(path);
            }

            ClearFolder(uploadFolderPath);

            return NoContent();
        }
    }
}