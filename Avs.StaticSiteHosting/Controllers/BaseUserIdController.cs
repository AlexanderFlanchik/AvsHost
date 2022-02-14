using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [ApiExceptionHandleFilter]
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

    public class ApiExceptionHandleFilter : Attribute, IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            context.ExceptionHandled = true;
            var errMessage = context.Exception.Message;
            var problemDetails = new ProblemDetails()
            {
                Title = "Server Error",
                Detail = errMessage,
                Status = 500
            };

            context.Result = new ObjectResult(problemDetails);

            return Task.CompletedTask;
        }
    }
}