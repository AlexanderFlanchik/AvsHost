using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.DTOs
{
    public class SiteDetailsModel
    {
        [Required]
        public string SiteName { get; set; }

        public string UploadSessionId { get; set; }
        
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public IDictionary<string, string> ResourceMappings { get; set; }               
        public string LandingPage { get; set; }
    }

    public class SiteDetailsResponse : SiteDetailsModel
    {
        public List<ContentItemModel> Uploaded { get; set; }
    }
}