﻿using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ContentItemModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string DestinationPath { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}