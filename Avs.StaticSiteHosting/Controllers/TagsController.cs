using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TagsController : BaseController
    {
        private readonly ITagsService _tagsService;

        public TagsController(ITagsService tagsService)
        {
            _tagsService = tagsService ?? throw new ArgumentNullException(nameof(tagsService));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagModel>>> GetTags() => Ok(await _tagsService.GetAllTags(CurrentUserId));

        [HttpGet("{tagId}")]
        public async Task<ActionResult<TagModel>> GetTagById(string tagId) => Ok(await _tagsService.GetTagById(CurrentUserId, tagId));

        [HttpPost]
        public async Task<IActionResult> CreateTag(TagModel newTagRequest)
        {
            var newTag = await _tagsService.CreateTag(CurrentUserId, newTagRequest);

            return CreatedAtAction(nameof(GetTagById), new { tagId = newTag.Id }, newTag);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTag(string tagId)
        {
            await _tagsService.DeleteTag(tagId);

            return NoContent();
        }

        [HttpGet]
        [Route("check-new-tag")]
        public async Task<IActionResult> NewTagExists(string tagName) => Json(await _tagsService.TagExists(CurrentUserId, tagName));
    }
}