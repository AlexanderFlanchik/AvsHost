using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.DTOs
{
    public class ToggleSiteStatusRequestModel
    {
        [Required]
        public string SiteId { get; set; }
    }
}