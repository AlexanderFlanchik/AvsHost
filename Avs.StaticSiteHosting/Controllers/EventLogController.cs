using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Http;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EventLogController : BaseController
    {
        [HttpPost]
        public async Task<IActionResult> GetEventLog(EventLogsQuery query, IUserService userService, IEventLogsService eventLogs)
        {           
            query.CurrentUserId = await userService.IsAdminAsync(CurrentUserId) ? null : CurrentUserId;

            var (
                totalEvents, 
                siteEvents
            ) = await eventLogs.GetEventLogsAsync(query);

            Response.Headers.Append(GeneralConstants.TOTAL_ROWS_AMOUNT, new StringValues(totalEvents.ToString()));

            return Ok(siteEvents);
        }
    }
}