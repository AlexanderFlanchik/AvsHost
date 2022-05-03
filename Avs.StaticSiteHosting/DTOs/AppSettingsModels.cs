using Avs.StaticSiteHosting.Web.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class AppSettingsModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// ViewModel for AppSettings page
    /// </summary>
    public class AppSettingsResponse
    {
        public CloudStorageSettings CloudStorage { get; set; }
        public SelectListItem[] CloudRegions { get; set; }

        public AppSettingsResponse()
        {
            CloudStorage = new CloudStorageSettings();
        }
    }

    /// <summary>
    /// DTO for AppSettings save operation
    /// </summary>
    public class AppSettingsSaveModel
    {
        public CloudStorageSettings CloudStorage { get; set; }
        
        public AppSettingsSaveModel()
        {
            CloudStorage = new CloudStorageSettings();
        }
    }
}