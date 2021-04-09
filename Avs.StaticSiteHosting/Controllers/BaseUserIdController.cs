using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private string _currentUserId;
        public string CurrentUserId => _currentUserId;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new BadRequestResult();
            }

            _currentUserId = userId;
        }
    }
}