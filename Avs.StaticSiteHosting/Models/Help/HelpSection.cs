﻿namespace Avs.StaticSiteHosting.Web.Models
{
    public class HelpSection: RoleAccessEntity
    {
        public string ParentSectionId { get; set; } 
        public string ExternalID { get; set; }
    }
}