using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class EditContentModel
    {
        public string Content { get; set; }
        public TimeSpan? CacheDuration { get; set; }
    }
}