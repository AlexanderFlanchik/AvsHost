using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Common;

public static class JsonDocumentExtensions
{
    public static MemoryStream ToStream(this JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);
        
        var stream = new MemoryStream();
        using var requestBodyWriter = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        jsonDocument.WriteTo(requestBodyWriter);
        requestBodyWriter.Flush();
        stream.Position = 0;
        
        return stream;
    }
}