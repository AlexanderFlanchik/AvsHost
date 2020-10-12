using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ToggleSiteStatusRequestModel
    {
        [Required]
        public string SiteId { get; set; }
    }
}