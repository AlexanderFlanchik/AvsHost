using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ConversationMessagesController : ControllerBase
    {
        private readonly IConversationMessagesService _conversationMessagesService;
      
        public ConversationMessagesController(IConversationMessagesService conversationMessagesService)
        {
            _conversationMessagesService = conversationMessagesService ?? throw new ArgumentNullException(nameof(conversationMessagesService));
        }

        [HttpGet("{conversationId}")]
        public async Task<ActionResult> GetConversationMessages([Required] string conversationId, int pageNumber, int pageSize)
            => Ok(await _conversationMessagesService.GetConversationMessagesAsync(conversationId, pageNumber, pageSize));

        [HttpPost]
        public async Task<IActionResult> SendMessage(CreateConversationMessageModel message, [FromServices] IUserRoleService userRoleService)
        {
            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var services = HttpContext.RequestServices;
            var conversation = (IHubContext<ConversationMessagesHub>)services.GetService(typeof(IHubContext<ConversationMessagesHub>));
            
            var createdMessage = await _conversationMessagesService.CreateConversationMessage(
                    userId,
                    message.ConversationId,
                    message.Content);

            if (createdMessage == null)
            {
                return BadRequest($"Conversation not found for ID = {message.ConversationId}");
            }

            // Send notification to Admin UI through SignalR
            var receiverIds = await userRoleService.GetAdminUserIds().ConfigureAwait(false);
            await conversation.Clients.Users(receiverIds).SendAsync("new-conversation-message", createdMessage);

            return Ok(createdMessage);
        }        
    }
}