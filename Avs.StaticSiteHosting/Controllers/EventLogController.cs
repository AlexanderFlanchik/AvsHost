using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EventLogController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> GetEventLog(EventLogsQuery query, [FromServices] IUserService userService, [FromServices] IEventLogsService eventLogs)
        {
            var currentUser = await userService.GetUserByIdAsync(CurrentUserId);
            if (currentUser is null)
            {
                return BadRequest("Invalid user ID passed.");
            }

            bool isAdmin = userService.IsAdmin(currentUser);
            query.CurrentUserId = !isAdmin ? CurrentUserId : null;

            var (totalEvents, siteEvents) = await eventLogs.GetEventLogsAsync(query);
            Response.Headers.Add(GeneralConstants.TOTAL_ROWS_AMOUNT, new StringValues(totalEvents.ToString()));

            return Ok(siteEvents);
        }
    }
}