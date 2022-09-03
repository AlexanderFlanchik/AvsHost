using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Avs.StaticSiteHosting.ContentCreator.Models;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement
{
    public interface IPageRenderingService
    {
        /// <summary>
        /// Renders HTML content as string using HTML tree data specified.
        /// </summary>
        /// <param name="htmlTree">HTML Tree data</param>
        /// <returns></returns>
        Task<string> RenderAsync(HtmlTreeRoot htmlTree);
    }

    public class PageRenderingService : IPageRenderingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRazorViewEngine _viewEngine;
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ITempDataProvider _tempDataProvider;

        public PageRenderingService(IServiceProvider serviceProvider, IRazorViewEngine viewEngine, IModelMetadataProvider modelMetadataProvider, ITempDataProvider tempDataProvider)
        {
            _serviceProvider = serviceProvider;
            _viewEngine = viewEngine;
            _modelMetadataProvider = modelMetadataProvider;
            _tempDataProvider = tempDataProvider;
        }

        public async Task<string> RenderAsync(HtmlTreeRoot htmlTree)
        {
            var httpContext = new DefaultHttpContext() { RequestServices = _serviceProvider };
            var modelState = new ModelStateDictionary();
            
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), modelState);
            var viewData = new ViewDataDictionary(_modelMetadataProvider, modelState);
            var tempData = new TempDataDictionary(httpContext, _tempDataProvider);

            var result = _viewEngine.FindView(actionContext, @"Templates/_HtmlTreeRoot", false);
            if (!result.Success)
            {
                throw new Exception("HTML tree template not found.");
            }

            using var writer = new StringWriter();
            viewData.Model = htmlTree;

            var viewContext = new ViewContext(actionContext, result.View, viewData, tempData, 
                writer, new HtmlHelperOptions());

            await result.View.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}