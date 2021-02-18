using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly IUserService _userService;

        public ConversationController(IConversationService conversationService, IUserService userService)
        {
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));            
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            var response = new { conversation = await _conversationService.GetConversationByAuthorID(userId) };
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var author = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            var newConversation = await _conversationService.CreateConversation(new ConversationModel { AuthorID = userId, Name = author.Name });
            
            return Ok(newConversation);
        }
    }
}