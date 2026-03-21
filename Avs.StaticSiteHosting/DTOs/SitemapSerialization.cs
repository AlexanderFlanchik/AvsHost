using System.Xml.Serialization;

namespace Avs.StaticSiteHosting.Web.DTOs;

public class Url
{
    [XmlElement("loc")]
    public string Location { get; set; }
    
    [XmlElement("modified")]
    public string Modified { get; set; }
}

[XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
public class UrlSet
{
    [XmlElement("url")]
    public Url[] Urls { get; set; }
}