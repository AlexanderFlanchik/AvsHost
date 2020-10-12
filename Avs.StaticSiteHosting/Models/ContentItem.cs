﻿using System;

namespace Avs.StaticSiteHosting.Web.Models
{
    public class ContentItem : BaseEntity
    {
        public Site Site { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FullName { get; set; }
    }
}