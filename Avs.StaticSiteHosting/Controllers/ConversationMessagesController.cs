using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
            var conversationHub = (IHubContext<ConversationMessagesHub>)services.GetService(typeof(IHubContext<ConversationMessagesHub>));

            var createdMessage = await _conversationMessagesService.CreateConversationMessage(
                    userId,
                    message.ConversationId,
                    message.Content);

            if (!message.IsAdminMessage)
            {
                // Send notification to Admin UI through SignalR
                var receiverIds = await userRoleService.GetAdminUserIds().ConfigureAwait(false);
                await conversationHub.Clients.Users(receiverIds).SendAsync(NEW_MESSAGE, createdMessage);
            }
            else
            {
                // Notify the user
                await conversationHub.Clients.User(conversation.AuthorID).SendAsync(NEW_MESSAGE, createdMessage);
            }

            return Ok(createdMessage);
        }        
    }
}