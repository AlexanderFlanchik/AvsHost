using System;
using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.Web.DTOs;

public record SavePageModel(
    [Required] string PreviewSessionId, 
    string ContentId, 
    string UploadSessionId, 
    string FileName,
    string DestinationPath,
    TimeSpan? CacheDuration
);

public record SavePageResponse(
    string Id,
    decimal Size,
    DateTime? UploadAt,
    DateTime? UpdateDate
);