namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// Wire framing format used by the streaming overloads on <see cref="IJsonRemapper"/>.
/// </summary>
public enum JsonStreamFormat
{
    /// <summary>
    /// Newline-delimited JSON: each chunk is a complete JSON document on its own line
    /// (e.g. Ollama, Gemini NDJSON mode).
    /// </summary>
    NewlineDelimitedJson,

    /// <summary>
    /// Server-Sent Events with <c>data: &lt;json&gt;\n\n</c> framing
    /// (e.g. OpenAI, Anthropic, Gemini SSE mode).
    /// </summary>
    ServerSentEvents
}
