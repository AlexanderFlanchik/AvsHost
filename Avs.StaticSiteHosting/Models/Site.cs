using System;
using System.Collections.Generic;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Tag = Avs.StaticSiteHosting.Web.Models.Tags.Tag;

namespace Avs.StaticSiteHosting.Web.Models
{
    public class Site : BaseEntity
    {
        public string Description { get; set; }
        public User CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LaunchedOn { get; set; }
        public IDictionary<string, string> Mappings { get; set; }
        public string LandingPage { get; set; }
        public EntityRef[] TagIds { get; set; }
        public Tag[] Tags { get; set; }
    }
}