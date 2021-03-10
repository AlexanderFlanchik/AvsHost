using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ConversationListController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationListController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task<IActionResult> Get(int pageNumber, int pageSize)
        {
            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            return Ok(await _conversationService.GetLatestConversations(pageNumber, pageSize, userId));
        }
    }
}