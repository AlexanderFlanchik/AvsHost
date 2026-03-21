using System;
using System.Linq;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ContentItemModel
    {
        private readonly string[] _editableFormats = [".txt", ".xml", ".xhtml", ".html", ".css", ".js"];
        private readonly string[] _viewableFormats = [".bmp", ".png", ".jpg", ".svg", ".gif"];
       
        public string Id { get; set; }
        public string FileName { get; set; }
        public string DestinationPath { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadedAt { get; set; }
        public decimal Size { get; set; }
        public DateTime? UpdateDate { get; set; }
        public TimeSpan? CacheDuration { get; set; }
        public bool IsEditable => _editableFormats.Any(f => FileName != null && FileName.EndsWith(f));
        public bool IsViewable => _viewableFormats.Any(f => FileName != null && FileName.EndsWith(f));
    }
}