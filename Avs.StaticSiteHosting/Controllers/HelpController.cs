using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HelpController : BaseController
    {
        private readonly IHelpContentService _helpService;

        public HelpController(IHelpContentService helpService)
        {
            _helpService = helpService ?? throw new ArgumentNullException(nameof(helpService));
        }

        [HttpGet]
        [Route("sections")]
        public async Task<ActionResult<IEnumerable<HelpSectionModel>>> GetHelpSections()
        {
            var roles = GetUserRoles();
            var helpSections = await _helpService.GetAllHelpSectionsAsync();

            return helpSections.Where(s =>
                s.RolesAllowed == null || s.RolesAllowed.Any(r => roles.Contains(r))
               ).ToList();
        }

        [HttpGet]
        [Route("GetHelpTopic")]
        public async Task<IActionResult> GetHelpTopic(string helpSectionId, int ordinalNo, [FromServices] ILogger<HelpController> logger)
        {
            if (string.IsNullOrEmpty(helpSectionId) || ordinalNo <= 0)
            {
                return BadRequest();
            }    

            var topic = await _helpService.GetTopicBySectionId(helpSectionId, ordinalNo);
            if (topic == null)
            {
                logger.LogWarning($"No help topic found for help section ID = '{helpSectionId}' and topic number = {ordinalNo}");

                return new EmptyResult();
            }

            var userRoles = GetUserRoles();
            var totalTopics = await _helpService.GetTopicsAmountAsync(helpSectionId);
            
            // Check role access if needed
            var allowedRoles = topic.RolesAllowed ?? Array.Empty<string>();
            if (allowedRoles.Any() && !userRoles.Any(r => allowedRoles.Contains(r)))
            {
                logger.LogWarning($"The help topic with ID = {topic.Id} is not available for user ID = {CurrentUserId}.");

                return new EmptyResult();
            }

            Response.Headers.Append("total-topics", new StringValues(totalTopics.ToString()));
            
            // Search for paragraphs available for a user
            var paragraphs = await _helpService.GetTopicContentAsync(topic.Id);
            var filteredParagraphs = paragraphs.Where(p =>
                    p.RolesAllowed == null || 
                    userRoles.Any(role => 
                            p.RolesAllowed.Contains(role))
             ).ToList();

            logger.LogInformation($"Help page requested ID = {topic.Id}, {filteredParagraphs.Count} paragraphs found.");
            topic.Paragraphs = filteredParagraphs;
                     
            return PartialView("HelpTopicContent", topic);
        }

        private string[] GetUserRoles() => 
                    User.Claims
                    .Where(r => r.Type == ClaimsIdentity.DefaultNameClaimType).Select(c => c.Value)
                    .ToArray();
    }
}