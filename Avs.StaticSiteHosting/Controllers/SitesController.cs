using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SitesController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SiteModel>>> GetSitesData([FromQuery] SiteRequestModel sitesRequest, ISiteService siteService)
        {           
            var order = sitesRequest.SortOrder;
            var query = new SitesQuery()
            {
                Page = sitesRequest.Page,
                PageSize = sitesRequest.PageSize,
                SortOrder = !string.IsNullOrEmpty(order) ? Enum.Parse<SortOrder>(order) : SortOrder.None,
                SortField = sitesRequest.SortField,
                TagIds = sitesRequest.TagIds
            };

            var claims = User.Claims;
            var roles = claims.Where(r => r.Type == ClaimsIdentity.DefaultRoleClaimType).ToArray();
            var isAdmin = roles.Any(r => r.Value == GeneralConstants.ADMIN_ROLE);

            string ownerUserID = !isAdmin ? CurrentUserId : null;                        
            query.OwnerId = ownerUserID;

            var amounts = await Task.WhenAll(new[]
                    {
                        siteService.GetSitesAmountAsync(ownerUserID),
                        siteService.GetActiveSitesAmountAsync(ownerUserID)
                    });
                       
            Response.Headers.Add(GeneralConstants.TOTAL_ROWS_AMOUNT, new StringValues(amounts[0].ToString()));
            Response.Headers.Add(GeneralConstants.ACTIVE_SITES_AMOUNT, new StringValues(amounts[1].ToString()));

            var sitesList = await siteService.GetSitesAsync(query);

            return Ok(sitesList);
        }
    }
}