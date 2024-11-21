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
using Avs.StaticSiteHosting.Shared.Common;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppSettingsController : ControllerBase
    {
        private readonly ISettingsManager _settingsManager;
        private readonly CloudStorageSettings _storageSettings;

        public AppSettingsController(ISettingsManager settingsManager, CloudStorageSettings storageSettings)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _storageSettings = storageSettings ?? throw new ArgumentNullException(nameof(storageSettings));
        }

        [HttpGet]
        public IActionResult Index()
        {
            var settings = new AppSettingsResponse()
            {
                CloudRegions = AWSHelper.GetAWSRegions()
                  .Select(r => new SelectListItem { Text = r.Value, Value = r.Key })
                  .ToArray(),

                CloudStorage = _storageSettings
            };

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

            _storageSettings.CopyFrom(cloudSettings);

            return NoContent();
        }
    }
}