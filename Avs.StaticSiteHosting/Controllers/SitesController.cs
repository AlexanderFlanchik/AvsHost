using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SitesController : ControllerBase
    {
        public IActionResult Get()
        {            
            return Ok(
                new[] { 
                    new SiteModel { 
                        Id = "1",
                        Name = "Site 1",
                        Description = "Test site #1",
                        LaunchedOn = DateTime.UtcNow.AddDays(-1),
                        IsActive = true
                    },
                    new SiteModel {
                        Id = "2",
                        Name = "Site 2",
                        Description = "Test site #2",
                        LaunchedOn = DateTime.UtcNow.AddDays(-2),
                        IsActive = true
                    },
                    new SiteModel {
                        Id = "3",
                        Name = "Site 3",
                        Description = "Test site #3",
                        LaunchedOn = DateTime.UtcNow.AddDays(-3),
                        IsActive = false
                    }
                }
            );
        }
    }
}