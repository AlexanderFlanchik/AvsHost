using System;
using System.Collections.Generic;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Models.SiteStatistics;
using Tag = Avs.StaticSiteHosting.Web.Models.Tags.Tag;

namespace Avs.StaticSiteHosting.Web.Models
{
    public class Site : BaseEntity
    {
        public string Description { get; set; }
        public User CreatedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LaunchedOn { get; set; }
        public DateTime? LastStopped { get; set; }
        public IDictionary<string, string> Mappings { get; set; }
        public string LandingPage { get; set; }
        public EntityRef[] TagIds { get; set; }
        public Tag[] Tags { get; set; }
        public string DatabaseName { get; set; }

        public List<EntityRef> CustomHandlerIds { get; set; } = [];

        #region Navigation properties
        
        public IList<ContentItem> ContentItems { get; set; }
        public IList<ViewedSiteInfo> Visits { get; set; }
        public IList<CustomRouteHandler> CustomRouteHandlers { get; set; }

        #endregion
    }
}