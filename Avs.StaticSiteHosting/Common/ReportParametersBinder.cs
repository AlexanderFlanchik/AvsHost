using Avs.StaticSiteHosting.Reports.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Common
{
    /// <summary>
    /// Binds a ReportParameters instance from a request.
    /// </summary>
    public class ReportParametersBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var reportParametersJson = bindingContext.HttpContext.Request.Form["reportParameters"];
            if (string.IsNullOrEmpty(reportParametersJson))
            {
                return Task.CompletedTask;
            }

            var reportParameters = new ReportParameters();
            reportParameters.SiteOwnerId = bindingContext.HttpContext.User.FindFirst(AuthSettings.UserIdClaim)?.Value;

            if (!string.IsNullOrEmpty(reportParametersJson))
            {
                var filters = JObject.Parse(reportParametersJson);

                foreach (var kvp in filters)
                {
                    reportParameters[kvp.Key] = kvp.Value;
                }
            }

            bindingContext.Result = ModelBindingResult.Success(reportParameters);

            return Task.CompletedTask;
        }
    }
}