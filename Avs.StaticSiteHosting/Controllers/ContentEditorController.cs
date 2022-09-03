using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Avs.StaticSiteHosting.ContentCreator.Models;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContentEditorController : Controller
    {
        private readonly IContentUploadService _contentUploadService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly ILogger<ContentEditorController> _logger;

        private readonly string[] render_formats = 
            { "text/plain", "text/html", "text/css", "text/javascript", "application/json", "application/javascript" };

        private const string CONTENT_CONTEXT_KEY = "edit-content-input-context";
        private const string PREVIEW_PREFIX = "page_preview_";

        public ContentEditorController(
            IContentManager contentManager,
            ISiteService siteService,
            ILogger<ContentEditorController> logger,
            IContentUploadService contentUploadService)
        {
            _contentManager = contentManager;
            _siteService = siteService;
            _logger = logger;
            _contentUploadService = contentUploadService;
        }

        [HttpGet]
        [Route("render-content")]
        public async Task<IActionResult> RenderContent([Required] string contentItemId)
        {
            var (_, contentType, contentStream) = await _contentManager.GetContentFileAsync(contentItemId);

            return render_formats.Contains(contentType) && contentStream is not null ?
                File(contentStream, contentType) : Content("This content cannot be prerendered.");
        }

        [HttpPost]
        [Route("store-content-context")]
        public IActionResult StoreContentInputData(ContentInputContext ctx)
        {
            HttpContext.Session.SetString(CONTENT_CONTEXT_KEY, JsonConvert.SerializeObject(ctx));
            
            return Ok();
        }

        [HttpGet]
        [Route("get-content-context")]
        public ActionResult<ContentInputContext> ReadContentInputData()
        {
            var ctxData = HttpContext.Session.GetString(CONTENT_CONTEXT_KEY);
            if (string.IsNullOrEmpty(ctxData))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<ContentInputContext>(ctxData);
        }

        [HttpPost]
        [Route("clear-editor-context")]
        public IActionResult ClearContentInputData(ClearEditorContextModel model)
        {
            HttpContext.Session.Remove(CONTENT_CONTEXT_KEY);
            if (!string.IsNullOrEmpty(model.PreviewSessionId))
            {
                HttpContext.Session.Remove($"{PREVIEW_PREFIX}{model.PreviewSessionId}");
            }
            
            return Ok();
        }

        [HttpPost]
        [Route("store-preview-session/{previewSessionId}")]
        public IActionResult StorePreviewSession(string previewSessionId, HtmlTreeRoot htmlTree)
        {
            _logger.LogInformation("New preview session has started, ID='{0}'.", previewSessionId);

            HttpContext.Session.SetString($"{PREVIEW_PREFIX}{previewSessionId}", JsonConvert.SerializeObject(htmlTree));    
            return Ok();
        }

        [HttpGet]
        [Route("page-preview")]
        public async Task<IActionResult> PagePreview([FromQuery] PagePreviewModel previewModel, [FromServices] IPagePreviewService previewService)
        {
            var previewSessionId = previewModel.PreviewSessionId;
            var contentId = previewModel.ContentId;
               
            if (string.IsNullOrEmpty(previewSessionId) && string.IsNullOrEmpty(contentId))
            {
                return PartialView("NoPreview");
            }

            _logger.LogInformation("Generating a page preview for request: {0}", JsonConvert.SerializeObject(previewModel));

            if (string.IsNullOrEmpty(previewSessionId))
            {
                var previewContent = await previewService.GetPagePreviewAsync(contentId);

                return string.IsNullOrEmpty(previewContent) ? PartialView("NoPreview") : Content(previewContent, "text/html");
            }

            var htmlTreeJsonFromSession = HttpContext.Session.GetString($"{PREVIEW_PREFIX}{previewSessionId}");
            if (string.IsNullOrEmpty(htmlTreeJsonFromSession))
            {
                return PartialView("NoPreview");
            }

            var previewViewModel = new PagePreviewViewModel() { UploadSessionId = previewModel.UploadSessionId };
            try
            {
                previewViewModel.SiteName = (await _siteService.GetSiteByIdAsync(previewModel.SiteId))?.Name;
                previewViewModel.HtmlTree = JsonConvert.DeserializeObject<HtmlTreeRoot>(htmlTreeJsonFromSession);
                
                _logger.LogInformation("HTML page preview with ID ='{0}' has been generated successfully.", previewSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to generate preview for ID = '{0}'", previewSessionId);
                
                return PartialView("PreviewError");
            }
                   
            return PartialView(previewViewModel);
        }

        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> SavePage(SavePageModel savePageModel, [FromServices] IPageRenderingService pageRenderingService)
        {
            string contentId = savePageModel.ContentId, uploadSessionId = savePageModel.UploadSessionId;
            var htmlDocumentJson = HttpContext.Session.GetString($"{PREVIEW_PREFIX}{savePageModel.PreviewSessionId}");

            if (string.IsNullOrEmpty(contentId) && string.IsNullOrEmpty(uploadSessionId))
            {
                _logger.LogWarning("At least one of the content ID or upload session ID must be non empty.");

                return BadRequest();
            }
            
            HtmlTreeRoot htmlTree;
            try
            {
                htmlTree = JsonConvert.DeserializeObject<HtmlTreeRoot>(htmlDocumentJson);
            }
            catch
            {
                _logger.LogError("Invalid HTML tree data, unable to parse from JSON.");
                
                return UnprocessableEntity();
            }

            SavePageResponse result = null;
            var content = await pageRenderingService.RenderAsync(htmlTree);
            if (!string.IsNullOrEmpty(contentId))
            {
                var contentSize = Math.Round((decimal) await _contentManager.UpdateContentItem(contentId, content) / 1024, 2);
                result = new SavePageResponse(contentId, contentSize, null, DateTime.UtcNow);

                _logger.LogInformation("The content with ID = '{0}' has been successfully updated.", contentId);
            }
            else
            {
                var destinationPath = savePageModel.DestinationPath;
                var fileName = savePageModel.FileName;
                if (!_contentUploadService.ValidateDestinationPath(destinationPath))
                {
                    return BadRequest("Invalid destination path. Network or relative paths are not allowed.");
                }

                await _contentUploadService.UploadContent(uploadSessionId, fileName, destinationPath, content);
                var siteContextData = HttpContext.Session.GetString(GeneralConstants.SITE_CONTEXT_KEY);
                if (!string.IsNullOrEmpty(siteContextData))
                {
                    SiteContextModel model = JsonConvert.DeserializeObject<SiteContextModel>(siteContextData);
                    var contentSize = Math.Round((decimal)_contentManager.GetNewFileSize(fileName, uploadSessionId) / 1024, 2);
                    model.UploadedFiles.Add(new ContentItemModel
                    {
                        FileName = fileName,
                        DestinationPath = savePageModel.DestinationPath,
                        Size = contentSize,
                        ContentType = _contentManager.GetContentType(fileName)
                    });

                    HttpContext.Session.SetString(GeneralConstants.SITE_CONTEXT_KEY, JsonConvert.SerializeObject(model));
                    result = new SavePageResponse(null, contentSize, DateTime.UtcNow, null);
                }
                
                _logger.LogInformation("New content file '{0}' has been saved successfully.", savePageModel.FileName);
            }

            return Ok(result);
        }
    }
}