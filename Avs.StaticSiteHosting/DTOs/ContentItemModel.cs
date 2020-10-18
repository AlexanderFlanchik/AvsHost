using System;
using System.Linq;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ContentItemModel
    {
        private readonly string[] EditableFormats = new string[] { ".txt", ".xml", ".xhtml", ".html", ".css", ".js" };
        private readonly string[] ViewableFormats = new string[] { ".bmp", ".png", ".jpg", ".svg" };
        
        public string Id { get; set; }
        public string FileName { get; set; }
        public string DestinationPath { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
        public decimal Size { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool IsEditable => EditableFormats.Any(f => FileName != null && FileName.EndsWith(f));
        public bool IsViewable => ViewableFormats.Any(f => FileName != null && FileName.EndsWith(f));
    }
}