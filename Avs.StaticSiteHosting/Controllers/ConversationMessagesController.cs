using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> SendMessage(CreateConversationMessageModel message)                    
            => Ok(await _conversationMessagesService.CreateConversationMessage(
                    User.FindFirst(AuthSettings.UserIdClaim)?.Value, 
                    message.ConversationId, 
                    message.Content));
        
    }
}