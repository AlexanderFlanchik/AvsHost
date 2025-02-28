using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ContentUploadRequest
    {
        [Required]
        public IFormFile ContentFile {  get; set; }
        public TimeSpan? CacheDuration { get; set; }
    }
}