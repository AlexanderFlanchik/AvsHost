using System.Net;

namespace Avs.StaticSiteHosting.ContentHost.Common;

public static class StreamWriterHelper
{
    private const string CONTENT_TYPE_DEFAULT = "application/octet-stream";
    
    public static async Task WriteStreamAsync(Stream stream, HttpResponse response, string? contentType)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = contentType ?? CONTENT_TYPE_DEFAULT;
        await stream.CopyToAsync(response.Body);
        stream.Position = 0;

        await response.Body.FlushAsync();
    }
}