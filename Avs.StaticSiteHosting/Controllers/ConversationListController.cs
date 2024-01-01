using Avs.StaticSiteHosting.Web.Services.AdminConversation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ConversationListController : BaseController
    {
        private readonly IConversationService _conversationService;

        public ConversationListController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task<IActionResult> Get(int pageNumber, int pageSize)
        {                       
            var (total, conversations) = await _conversationService.GetLatestConversations(pageNumber, pageSize, CurrentUserId);
            Response.Headers.Append("total-conversations", new StringValues(total.ToString()));

            return Ok(conversations);
        }
    }
}