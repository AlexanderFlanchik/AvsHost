using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ConversationController : BaseController
    {
        private readonly IConversationService _conversationService;
        private readonly IUserService _userService;

        public ConversationController(IConversationService conversationService, IUserService userService)
        {
            _conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));            
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(new { conversation = await _conversationService.GetConversationByAuthorID(CurrentUserId) });

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> Get(string conversationId)
           => Ok(new { conversation = await _conversationService.GetConversationById(conversationId) });
                
        [HttpPost]
        public async Task<IActionResult> Create()
        {            
            var author = await _userService.GetUserByIdAsync(CurrentUserId).ConfigureAwait(false);
            var newConversation = await _conversationService.CreateConversation(new ConversationModel { AuthorID = CurrentUserId, Name = author.Name });
            
            return Ok(newConversation);
        }

        [HttpGet]
        [Route("unread")]
        public async Task<IActionResult> AreUnreadConversations() => Ok(await _conversationService.AnyUserUnreadConversations(CurrentUserId));

        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> SearchConversation(SearchConversationModel searchRequest)
            => Ok(await _conversationService.SearchConversationsByName(searchRequest.SearchName, searchRequest.IgnoreConversationIds));
    }
}