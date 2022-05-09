using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Services.Settings;
using Microsoft.AspNetCore.Authorization;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppSettingsController : ControllerBase
    {
        private readonly ISettingsManager _settingsManager;

        public AppSettingsController(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cloudSettingsJson = (await _settingsManager.GetAsync(CloudStorageSettings.SettingsName))?.Value;
            var settings = new AppSettingsResponse()
            {
                CloudRegions = AWSHelper.GetAWSRegions()
                  .Select(r => new SelectListItem { Text = r.Value, Value = r.Key })
                  .ToArray()
            };

            if (!string.IsNullOrEmpty(cloudSettingsJson))
            {
                settings.CloudStorage = JsonConvert.DeserializeObject<CloudStorageSettings>(cloudSettingsJson);
            }

            return Ok(settings);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSettings(AppSettingsSaveModel settingsModel)
        {
            var cloudSettings = settingsModel.CloudStorage;
            if (cloudSettings is null)
            {
                return BadRequest("Cloud settings must be specified.");
            }

            var cloudSettingsJson = JsonConvert.SerializeObject(cloudSettings);

            await _settingsManager.UpdateOrAddAsync(CloudStorageSettings.SettingsName, cloudSettingsJson, "Cloud Storage settings");

            return NoContent();
        }
    }
}