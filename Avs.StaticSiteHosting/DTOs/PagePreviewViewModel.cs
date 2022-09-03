using Avs.StaticSiteHosting.ContentCreator.Models;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class PagePreviewViewModel
    {
        public string UploadSessionId { get; set; }
        public string SiteName { get; set; }
        public HtmlTreeRoot HtmlTree { get; set; }
    }
}