using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentUploadController : ControllerBase
    {
        private readonly StaticSiteOptions staticSiteOptions;
        public ContentUploadController(IOptions<StaticSiteOptions> staticSiteOptions)
        {
            this.staticSiteOptions = staticSiteOptions.Value;
        }

        [Route("session")]
        public IActionResult StartUploadSession()
        {
            Response.Headers.Add(GeneralConstants.UPLOAD_SESSION_ID, Guid.NewGuid().ToString());

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

                Console.WriteLine(ex.Message);
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
                Console.WriteLine(ex.Message);
                return Problem($"Unable to upload {contentFile.FileName}. Server error.");
            }

            return Ok();
        }
    }
}