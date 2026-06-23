using System.Buffers;
using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// One parsed Server-Sent-Events event: an optional event name and its <c>data:</c> payload bytes.
/// <c>Data</c> is a slice into the source stream buffer — no copy.
/// </summary>
public readonly struct SseEvent
{
    public SseEvent(ReadOnlyMemory<byte> eventName, ReadOnlyMemory<byte> data)
    {
        EventName = eventName;
        Data = data;
    }

    public ReadOnlyMemory<byte> EventName { get; }
    public ReadOnlyMemory<byte> Data { get; }
    public bool IsDone => Data.Span.SequenceEqual("[DONE]"u8);
}

/// <summary>
/// Helpers for reading and writing streaming wire formats (SSE + NDJSON), and for routing each
/// frame through a per-event-type mapping.
/// </summary>
public static class StreamReformatter
{
    private static ReadOnlySpan<byte> SseDataPrefix => "data: "u8;
    private static ReadOnlySpan<byte> SseEventPrefix => "event: "u8;
    private static ReadOnlySpan<byte> SseFrameTerminator => "\n\n"u8;
    private static ReadOnlySpan<byte> SseDone => "data: [DONE]\n\n"u8;
    private static ReadOnlySpan<byte> Newline => "\n"u8;

    // --------------------------------------------------------------------------------
    // NDJSON
    // --------------------------------------------------------------------------------

    /// <summary>Iterate over the non-empty JSON lines of an NDJSON stream. Slices into source.</summary>
    public static IEnumerable<ReadOnlyMemory<byte>> ParseNdjson(ReadOnlyMemory<byte> stream)
    {
        int i = 0;
        while (i < stream.Length)
        {
            int lineStart = i;
            while (i < stream.Length && stream.Span[i] != (byte)'\n') i++;
            int lineEnd = i;
            if (lineEnd > lineStart && stream.Span[lineEnd - 1] == (byte)'\r') lineEnd--;
            if (lineEnd > lineStart)
                yield return stream[lineStart..lineEnd];
            if (i < stream.Length) i++;
        }
    }

    // --------------------------------------------------------------------------------
    // SSE
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Iterate over the events of an SSE stream. Each event is the accumulated <c>event:</c> /
    /// <c>data:</c> lines between blank-line separators. Multi-line <c>data:</c> payloads (which
    /// the spec says should be joined with '\n') are not common in LLM streams; we concatenate
    /// them when seen.
    /// </summary>
    public static IEnumerable<SseEvent> ParseSse(ReadOnlyMemory<byte> stream)
    {
        int i = 0;
        ReadOnlyMemory<byte> currentEvent = default;
        // Single-data-line case is overwhelmingly the norm; multi-line we lazily build a pooled buffer.
        byte[]? scratch = null;
        int scratchLen = 0;
        ReadOnlyMemory<byte> firstData = default;
        bool hasData = false;

        try
        {
            while (i <= stream.Length)
            {
                // Find end-of-line.
                int lineStart = i;
                while (i < stream.Length && stream.Span[i] != (byte)'\n') i++;
                int lineEnd = i;
                if (lineEnd > lineStart && stream.Span[lineEnd - 1] == (byte)'\r') lineEnd--;

                if (lineStart == lineEnd)
                {
                    // Blank line → end of event.
                    if (hasData)
                    {
                        ReadOnlyMemory<byte> dataOut;
                        if (scratch is null)
                        {
                            dataOut = firstData;
                        }
                        else
                        {
                            dataOut = new ReadOnlyMemory<byte>(scratch, 0, scratchLen);
                        }
                        yield return new SseEvent(currentEvent, dataOut);
                    }
                    currentEvent = default;
                    firstData = default;
                    hasData = false;
                    if (scratch is not null)
                    {
                        ArrayPool<byte>.Shared.Return(scratch);
                        scratch = null;
                        scratchLen = 0;
                    }
                }
                else
                {
                    var line = stream[lineStart..lineEnd];
                    if (line.Span.StartsWith(SseEventPrefix))
                    {
                        currentEvent = line[SseEventPrefix.Length..];
                    }
                    else if (line.Span.StartsWith(SseDataPrefix))
                    {
                        var payload = line[SseDataPrefix.Length..];
                        if (!hasData)
                        {
                            firstData = payload;
                            hasData = true;
                        }
                        else
                        {
                            EnsureScratch(ref scratch, ref scratchLen, firstData, payload);
                            firstData = default;
                        }
                    }
                    // Other lines (id:, retry:, comments starting with ':') ignored for LLM use cases.
                }

                if (i >= stream.Length) break;
                i++;
            }

            // Trailing event without blank-line terminator (rare but tolerated).
            if (hasData)
            {
                ReadOnlyMemory<byte> dataOut = scratch is null
                    ? firstData
                    : new ReadOnlyMemory<byte>(scratch, 0, scratchLen);
                yield return new SseEvent(currentEvent, dataOut);
            }
        }
        finally
        {
            if (scratch is not null) ArrayPool<byte>.Shared.Return(scratch);
        }
    }

    private static void EnsureScratch(ref byte[]? scratch, ref int scratchLen,
        ReadOnlyMemory<byte> first, ReadOnlyMemory<byte> next)
    {
        if (scratch is null)
        {
            scratch = ArrayPool<byte>.Shared.Rent(first.Length + 1 + next.Length + 64);
            first.Span.CopyTo(scratch);
            scratchLen = first.Length;
        }
        int need = scratchLen + 1 + next.Length;
        if (need > scratch.Length)
        {
            var bigger = ArrayPool<byte>.Shared.Rent(Math.Max(scratch.Length * 2, need));
            scratch.AsSpan(0, scratchLen).CopyTo(bigger);
            ArrayPool<byte>.Shared.Return(scratch);
            scratch = bigger;
        }
        scratch[scratchLen++] = (byte)'\n';
        next.Span.CopyTo(scratch.AsSpan(scratchLen));
        scratchLen += next.Length;
    }

    // --------------------------------------------------------------------------------
    // Writers
    // --------------------------------------------------------------------------------

    /// <summary>Write a <c>data: &lt;json&gt;\n\n</c> SSE frame to <paramref name="output"/>.</summary>
    public static void WriteSseFrame(IBufferWriter<byte> output, ReadOnlySpan<byte> json)
    {
        var span = output.GetSpan(SseDataPrefix.Length + json.Length + SseFrameTerminator.Length);
        SseDataPrefix.CopyTo(span);
        json.CopyTo(span[SseDataPrefix.Length..]);
        SseFrameTerminator.CopyTo(span[(SseDataPrefix.Length + json.Length)..]);
        output.Advance(SseDataPrefix.Length + json.Length + SseFrameTerminator.Length);
    }

    /// <summary>Write the OpenAI-style terminator <c>data: [DONE]\n\n</c>.</summary>
    public static void WriteSseDone(IBufferWriter<byte> output)
    {
        var span = output.GetSpan(SseDone.Length);
        SseDone.CopyTo(span);
        output.Advance(SseDone.Length);
    }

    /// <summary>Append a JSON document followed by '\n' (one NDJSON line).</summary>
    public static void WriteNdjsonLine(IBufferWriter<byte> output, ReadOnlySpan<byte> json)
    {
        var span = output.GetSpan(json.Length + 1);
        json.CopyTo(span);
        span[json.Length] = (byte)'\n';
        output.Advance(json.Length + 1);
    }

    // --------------------------------------------------------------------------------
    // Event-type routing
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Inspect a JSON chunk for a single string-valued field (e.g. <c>$.type</c>) and return its
    /// value. Returns null when the field is absent or non-string. Allocates one string per call
    /// for use as a dictionary key — acceptable here because the cardinality of event types is
    /// small and the lookup happens per-chunk, not per-byte.
    /// </summary>
    public static string? ReadDiscriminator(ReadOnlyMemory<byte> json, ReadOnlySpan<byte> fieldUtf8)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty(fieldUtf8, out var v) && v.ValueKind == JsonValueKind.String)
            return v.GetString();
        return null;
    }
}
