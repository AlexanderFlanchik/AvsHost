using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System;

namespace Avs.StaticSiteHosting.Web.Middlewares
{
    public class ResourcePreviewContentMiddleware : IMiddleware
    {
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.GetEncodedPathAndQuery();
            Console.WriteLine(path);
            if (!IsResourceContentPreviewRequest(path))
            {
                return next.Invoke(context);
            }

            var subQueryStart = path.IndexOf('?');
            path = path.Substring(0, subQueryStart);

            var query = context.Request.Query;
            var contentPath = HttpUtility.UrlEncode(path);

            context.Request.Path = "/api/ResourcePreviewContent";
            var queryParams = new Dictionary<string, StringValues>()
            {
                ["contentPath"] = new StringValues(contentPath),
                ["__accessToken"] = query["__accessToken"]
            };

            if (query["uploadSessionId"] != StringValues.Empty)
            {
                queryParams.Add("uploadSessionId", query["uploadSessionId"]);
            }
            else if (query["siteId"] != StringValues.Empty)
            {
                queryParams.Add("siteId", query["siteId"]);
            }

            context.Request.Query = new QueryCollection(queryParams);

            return next.Invoke(context);
        }

        private bool IsResourceContentPreviewRequest(string path)
            => path.Contains($"{GeneralConstants.NEW_RESOURCE_PATTERN}") ||
               path.Contains($"{GeneralConstants.EXIST_RESOURCE_PATTERN}");
    }
}