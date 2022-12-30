using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    public class FooterController : Controller
    {
        [HttpGet]
        public IActionResult Index() => PartialView("__FooterPartial");
    }
}