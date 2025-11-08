using System;

namespace Avs.StaticSiteHosting.Web.DTOs;

public class SiteModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? LaunchedOn { get; set; }
    public UserModel Owner { get; set; }
    public bool IsActive { get; set; }
    public string LandingPage { get; set; }
    public string DatabaseName { get; set; }
    public TagModel[] Tags { get; set; }
}

public class UserModel
{
    public string Id { get; set; }
    public string UserName { get; set; }
}