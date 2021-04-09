using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.Identity;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ConversationMessagesController : ControllerBase
    {
        private const string NEW_MESSAGE = "new-conversation-message";
        private readonly IConversationMessagesService _conversationMessagesService;
        private readonly IConversationService _conversationService;
        public ConversationMessagesController(IConversationMessagesService conversationMessagesService, IConversationService conversationService)
        {
            _conversationMessagesService = conversationMessagesService ?? throw new ArgumentNullException(nameof(conversationMessagesService));
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
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

            var conversation = await _conversationService.GetConversationById(message.ConversationId);
            if (conversation == null)
            {
                return BadRequest($"Conversation not found for ID = {message.ConversationId}");
            }

            var services = HttpContext.RequestServices;
            var notificationHub = (IHubContext<UserNotificationHub>)services.GetService(typeof(IHubContext<UserNotificationHub>));

            var createdMessage = await _conversationMessagesService.CreateConversationMessage(
                    userId,
                    message.ConversationId,
                    message.Content
            );

            if (!message.IsAdminMessage)
            {
                // Get IDs of Admin users
                var receiverIds = await userRoleService.GetAdminUserIds().ConfigureAwait(false);
                
                // Send notification to Admin UI through SignalR
                await notificationHub.Clients.Users(receiverIds).SendAsync(NEW_MESSAGE, createdMessage);
            }
            else
            {
                // Notify the user who created the conversation
                await notificationHub.Clients.User(conversation.AuthorID).SendAsync(NEW_MESSAGE, createdMessage);
            }

            return Ok(createdMessage);
        }

        [HttpPost]
        [Route("makeread")]
        public async Task<IActionResult> MakeMessagesRead(IEnumerable<string> messageIds)
        {
            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            await _conversationMessagesService.MakeMessagesRead(messageIds, userId);

            return Ok();
        }
    }
}