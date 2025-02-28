using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ContentCacheSettings
    {
        public string FileName { get; set; }
        public string DestinationPath { get; set; }
        public TimeSpan ContentDuration { get; set; }
    }
}
