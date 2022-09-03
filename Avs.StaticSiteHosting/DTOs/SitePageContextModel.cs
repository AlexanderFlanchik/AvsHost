using System.Collections.Generic;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class SiteContextModel
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string Description { get; set; }
        public string LandingPage { get; set; }
        public bool IsActive { get; set; }
        public ResourceMapping[] ResourceMappings { get; set; }
        public string UploadSessionId { get; set; }
        public List<ContentItemModel> UploadedFiles { get; set; } = new List<ContentItemModel>();
    }

    public class ResourceMapping
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}