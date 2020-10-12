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
    public class SitesController : ControllerBase
    {
        private readonly ISiteService _siteService;

        public SitesController(ISiteService siteService)
        {
            _siteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
        }

        public async Task<ActionResult<IEnumerable<SiteModel>>> GetSites(int page, int pageSize, string sortOrder, string sortField)
        {           
            var query = new SitesQuery()
            {
                Page = page,
                PageSize = pageSize,
                SortOrder = !string.IsNullOrEmpty(sortOrder) ? Enum.Parse<SortOrder>(sortOrder) : SortOrder.None,
                SortField = sortField
            };

            var claims = User.Claims;
            var roles = claims.Where(r => r.Type == ClaimsIdentity.DefaultRoleClaimType).ToArray();
            var isAdmin = roles.Any(r => r.Value == GeneralConstants.ADMIN_ROLE);

            int totalFound;
            if (!isAdmin)
            {
                var userId = claims.FirstOrDefault(r => r.Type == AuthSettings.UserIdClaim)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Invalid user.");
                }

                query.OwnerId = userId;
                totalFound = await _siteService.GetSitesAmountAsync(query.OwnerId);
            }
            else
            {
                totalFound = await _siteService.GetSitesAmountAsync();
            }
                        
            Response.Headers.Add(GeneralConstants.TOTAL_ROWS_AMOUNT, new StringValues(totalFound.ToString()));

            return Ok((await _siteService.GetSitesAsync(query))
                .Select(s => new SiteModel() { 
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    LaunchedOn = s.LaunchedOn,
                    IsActive = s.IsActive,
                    LandingPage = s.LandingPage
                }).ToArray());
        }
    }
}