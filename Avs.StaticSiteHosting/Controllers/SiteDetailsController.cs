using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class SiteDetailsController : BaseController
    {
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly IUserService _userService;

        public SiteDetailsController(ISiteService siteService, IUserService userService, IContentManager contentManager)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            _contentManager = contentManager ?? throw new ArgumentException(nameof(contentManager));
        }

        [HttpGet]
        [Route("CheckSiteName")]
        public async Task<IActionResult> ValidateSiteName(string siteName, string siteId)
            => Json(!await _siteService.CheckSiteNameUsedAsync(siteName, siteId));

        [HttpGet("{siteId}")]
        public async Task<ActionResult<SiteDetailsResponse>> GetSiteDetails(string siteId)
        {
            var site = await _siteService.GetSiteByIdAsync(siteId);
            if (site == null)
            {
                return NotFound();
            }

            // User can edit only own site installations
            if (CurrentUserId != site.CreatedBy.Id)
            {
                return Unauthorized();
            }

            var uploadedFiles = await _contentManager.GetUploadedContentAsync(siteId).ConfigureAwait(false);

            var siteDetailsResponse = new SiteDetailsResponse()
                {
                    SiteName = site.Name,
                    Description = site.Description,
                    IsActive = site.IsActive,
                    ResourceMappings = site.Mappings,
                    LandingPage = site.LandingPage,
                    Uploaded = uploadedFiles.ToList()
                };

            return siteDetailsResponse;
        }
                       
        [HttpPost]
        public async Task<IActionResult> CreateSite(SiteDetailsModel siteDetails, [FromServices] IEventLogsService eventLogsService)
        {
            var uploadSessionId = siteDetails.UploadSessionId;
            if (string.IsNullOrEmpty(uploadSessionId))
            {
                return BadRequest("No uploaded files found.");
            }

            if (await _siteService.CheckSiteNameUsedAsync(siteDetails.SiteName, null))
            {
                return Conflict("This site name is already in use.");
            }

            var currentUser = await _userService.GetUserByIdAsync(CurrentUserId).ConfigureAwait(false);
            if (currentUser == null)
            {
                return BadRequest("Cannot find user by ID provided.");
            }

            var siteData = new Site()
            {
                Name = siteDetails.SiteName,
                Description = siteDetails.Description,
                IsActive = siteDetails.IsActive,
                CreatedBy = currentUser,
                LaunchedOn = DateTime.UtcNow,
                Mappings = siteDetails.ResourceMappings,
                LandingPage = siteDetails.LandingPage
            };

            var newSite = await _siteService.CreateSiteAsync(siteData).ConfigureAwait(false);
            await _contentManager.ProcessSiteContentAsync(newSite, uploadSessionId).ConfigureAwait(false);

            await eventLogsService.InsertSiteEventAsync(newSite.Id, "Site Created", SiteEventType.Information,
                $"Site '{newSite.Name}' was created successfully.");

            var routeValues = new { siteId = newSite.Id };

            return CreatedAtAction(nameof(GetSiteDetails), routeValues, newSite);
        }

        [HttpPut("{siteId}")]
        public async Task<IActionResult> UpdateSite(string siteId, SiteDetailsModel siteDetails, [FromServices] IEventLogsService eventLogsService)
        {
            var currentUser = await _userService.GetUserByIdAsync(CurrentUserId);
            var siteToUpdate = await _siteService.GetSiteByIdAsync(siteId).ConfigureAwait(false);
            if (siteToUpdate == null)
            {
                return NotFound();
            }

            if (await _siteService.CheckSiteNameUsedAsync(siteDetails.SiteName, siteId))
            {
                return Conflict("This site name is already in use.");
            }
                                  
            if (siteToUpdate.CreatedBy.Id != CurrentUserId)
            {
                return Unauthorized();
            }

            siteToUpdate.Name = siteDetails.SiteName;
            bool siteStateChanged = siteToUpdate.IsActive != siteDetails.IsActive;
            if (siteStateChanged)
            {                
                siteToUpdate.IsActive = siteDetails.IsActive;
                if (siteToUpdate.IsActive)
                {
                    await eventLogsService.InsertSiteEventAsync(siteToUpdate.Id, "Site Started.", SiteEventType.Information, 
                        $"Site '{siteToUpdate.Name}' was started by {currentUser.Name}.");
                }
                else
                {
                    await eventLogsService.InsertSiteEventAsync(siteToUpdate.Id, "Site Stopped.", SiteEventType.Warning,
                         $"Site '{siteToUpdate.Name}' was stopped by {currentUser.Name}.");
                }
            }
            
            siteToUpdate.Description = siteDetails.Description;
            siteToUpdate.Mappings = siteDetails.ResourceMappings;
            siteToUpdate.LandingPage = siteDetails.LandingPage;

            await _siteService.UpdateSiteAsync(siteToUpdate).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(siteDetails.UploadSessionId))
            {
                await _contentManager.ProcessSiteContentAsync(siteToUpdate, siteDetails.UploadSessionId).ConfigureAwait(false);
            }
            
            return NoContent();
        }

        [HttpGet]
        [Route("content-get")]
        public async Task<IActionResult> GetContent([Required] string contentItemId, int? maxWidth, [FromServices] ImageResizeService resizeService)
        {
            var (
                fileName, 
                contentType, 
                contentStream
            ) = await _contentManager.GetContentFileAsync(contentItemId);
            contentType ??= "application/octet-stream";
            
            if (contentStream is null)
            {
                return NotFound();
            }
           
            Response.Headers.Add("content-disposition", $"attachment;filename={fileName}");

            if (maxWidth.HasValue) // its graphic content, possible we need to resize it to fit max width.
            {
                try
                {
                    var resizedImageStream = await resizeService.GetResizedImageStreamAsync(contentStream, contentType, maxWidth.Value);
                    
                    return File(resizedImageStream, contentType);
                }
                catch (InvalidOperationException)
                {
                    return BadRequest(ModelState); // the image is invalid
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
            await _contentManager.UpdateContentItem(contentItemId, model.Content).ConfigureAwait(false);
            
            return Ok();
        }

        [HttpDelete]
        [Route("content-delete")]
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
                isDeleted = await _contentManager.DeleteContentByIdAsync(contentItemId).ConfigureAwait(false);

                errorMsg = isDeleted ? string.Empty : $"Cannot delete content with ID: {contentItemId}";
            }
            else if (!string.IsNullOrEmpty(contentItemName))
            {
                isDeleted = _contentManager.DeleteNewUploadedFile(contentItemName, uploadSessionId);

                errorMsg = isDeleted ? string.Empty : $"No file deleted, content file name {contentItemName}, session ID: {uploadSessionId}.";
            }
            else
            {
                return BadRequest("You must specify either content item ID or name with upload session ID");
            }

            if (!isDeleted)
            {
                return BadRequest(new { errorMsg });
            }

            return Ok();
        }
    }
}