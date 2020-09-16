﻿using Avs.StaticSiteHosting.DTOs;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route(GeneralConstants.ERROR_ROUTE)]
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            var errorModel = new ErrorInfoModel();
            var errorFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (errorFeature.Error == null)
            {
                return Redirect("/");
            }

            var exception = errorFeature.Error as StaticSiteProcessingException;
            
            if (exception == null)
            {
                errorModel.ErrorMessage = "Unknown server error";
                errorModel.Title = "Error";
                Response.StatusCode = 500;
            }
            else
            {
                errorModel.ErrorMessage = exception.Message;
                errorModel.Title = exception.ErrorTitle;
                Response.StatusCode = exception.HttpStatusCode;
            }

            return View(errorModel);
        }
    }
}