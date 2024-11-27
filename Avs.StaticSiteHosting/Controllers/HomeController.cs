using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.SiteStatistics;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : BaseController
    {
        private readonly ISiteStatisticsService _siteStatisticsService;
        private readonly IEventLogsService _eventLogsService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        
        public HomeController(ISiteStatisticsService siteStatisticsService, 
            IEventLogsService eventLogsService, 
            IContentManager contentManager, 
            ISiteService siteService)
        {
            _siteStatisticsService = siteStatisticsService;
            _eventLogsService = eventLogsService;
            _contentManager = contentManager;
            _siteService = siteService;
        }

        [HttpGet]
        public async Task<ActionResult<HomePageModel>> Index()
        {
            var homeModel = new HomePageModel();            
            var siteAmounts = await Task.WhenAll(_siteService.GetSitesAmountAsync(CurrentUserId), 
                _siteService.GetActiveSitesAmountAsync(CurrentUserId)
            );

            // Site amounts - active and total
            homeModel.TotalSites = siteAmounts[0];
            homeModel.ActiveSites = siteAmounts[1];

            // We look for all error logs during the current day
            var dateFrom =  DateTime.UtcNow.Date;
            var dateTo = dateFrom.AddDays(1).AddMilliseconds(-1);
            
            var errorLogsQuery = new EventLogsQuery() 
                { 
                    CurrentUserId = CurrentUserId, 
                    Type = Models.SiteEventType.Error, 
                    DateFrom = dateFrom, 
                    DateTo = dateTo 
                };

            var (errors, _) = await _eventLogsService.GetEventLogsAsync(errorLogsQuery);
            homeModel.Errors = (int)errors;
            homeModel.TotalSiteVisits = await _siteStatisticsService.GetTotalSiteVisits(CurrentUserId);            
            homeModel.StorageUsedInfos = (await _contentManager.GetUsedStorageAmountByUser(CurrentUserId)).ToArray();

            return Ok(homeModel);
        }
    }
}