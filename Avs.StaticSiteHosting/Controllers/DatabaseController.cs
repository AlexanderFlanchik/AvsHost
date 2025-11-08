using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Services.Databases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DatabaseController(IDatabaseService databaseService) : BaseController
{
    [HttpGet]
    [Route("validate/{databaseName}")]
    public async Task<IActionResult> ValidateDatabaseName(string databaseName)
    {
        bool isValid = await databaseService.IsDatabaseNameUnique(databaseName, CurrentUserId);

        return Ok(new { isValid });
    }

    [HttpGet]
    public async Task<IActionResult> ListDatabases()
    {
        return Ok(await databaseService.GetUserDatabases(CurrentUserId));
    }
}