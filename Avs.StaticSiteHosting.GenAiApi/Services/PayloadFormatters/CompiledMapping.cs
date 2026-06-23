using System.Buffers;
using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

/// <summary>
/// An immutable, compiled mapping. Build it once with <see cref="MappingCompiler.Compile(string)"/>
/// and reuse for every request. All allocations (property names, paths, value-maps, literal bytes)
/// happen here; the runtime <c>Apply</c> path performs no schema work.
/// </summary>
public sealed class CompiledMapping
{
    private readonly IMappingNode _root;

    internal CompiledMapping(string name, string id, string? description, IMappingNode root)
    {
        Name = name;
        Id = id;
        Description = description;
        _root = root;
    }

    public string Name { get; }
    public string Id { get; }
    public string? Description { get; }

    /// <summary>
    /// Apply the mapping. Source is parsed via <see cref="JsonDocument"/> (uses pooled buffers
    /// from <see cref="ArrayPool{T}"/>), destination tokens are written directly to
    /// <paramref name="destination"/>. No intermediate string/object allocations are made for
    /// values being copied; <see cref="JsonElement.WriteTo"/> emits raw token bytes.
    /// </summary>
    public void Apply(ReadOnlyMemory<byte> sourceUtf8, IBufferWriter<byte> destination, JsonWriterOptions writerOptions = default)
    {
        using var doc = JsonDocument.Parse(sourceUtf8);
        using var writer = new Utf8JsonWriter(destination, writerOptions);
        _root.WriteValue(doc.RootElement, doc.RootElement, writer);
        writer.Flush();
    }

    /// <summary>
    /// Apply with caller-supplied (and reusable) <see cref="Utf8JsonWriter"/>.
    /// The writer is <see cref="Utf8JsonWriter.Reset(IBufferWriter{byte})"/>-ed onto the buffer
    /// before use, so the writer instance can be cached per-thread to avoid setup costs.
    /// </summary>
    public void Apply(ReadOnlyMemory<byte> sourceUtf8, IBufferWriter<byte> destination, Utf8JsonWriter cachedWriter)
    {
        using var doc = JsonDocument.Parse(sourceUtf8);
        cachedWriter.Reset(destination);
        _root.WriteValue(doc.RootElement, doc.RootElement, cachedWriter);
        cachedWriter.Flush();
    }

    /// <summary>
    /// Apply against a non-contiguous source buffer (e.g. <see cref="System.IO.Pipelines.PipeReader"/>
    /// output). Uses pooled buffers internally.
    /// </summary>
    public void Apply(ReadOnlySequence<byte> sourceUtf8, IBufferWriter<byte> destination, Utf8JsonWriter cachedWriter)
    {
        using var doc = JsonDocument.Parse(sourceUtf8);
        cachedWriter.Reset(destination);
        _root.WriteValue(doc.RootElement, doc.RootElement, cachedWriter);
        cachedWriter.Flush();
    }

    /// <summary>
    /// Apply against a pre-parsed source document. Use this when you already hold a
    /// <see cref="JsonDocument"/> (e.g. inspected the body for routing) and want to avoid re-parsing.
    /// </summary>
    public void Apply(JsonDocument source, IBufferWriter<byte> destination, Utf8JsonWriter cachedWriter)
    {
        cachedWriter.Reset(destination);
        _root.WriteValue(source.RootElement, source.RootElement, cachedWriter);
        cachedWriter.Flush();
    }
}
