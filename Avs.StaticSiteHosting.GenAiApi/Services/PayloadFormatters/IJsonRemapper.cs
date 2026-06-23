using System.Buffers;
using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public interface IJsonRemapper
{
    /// <summary>
    /// Register an already-compiled mapping.
    /// </summary>
    void Register(CompiledMapping mapping);

    /// <summary>
    /// Compile mapping JSON and register it. Returns the compiled mapping.
    /// </summary>
    CompiledMapping RegisterFromJson(string mappingJson, string mappingId);
    
    /// <summary>
    /// Try to fetch a registered mapping. Returns false when the name is unknown.
    /// </summary>
    bool TryGet(string mappingId, out CompiledMapping mapping);

    /// <summary>
    /// Fetch a registered mapping or throw <see cref="KeyNotFoundException"/>.
    /// </summary>
    CompiledMapping Get(string mappingId);

    /// <summary>
    /// Remove a registered mapping. Returns <c>true</c> when the mapping was present.
    /// Concurrent <c>Transform</c> calls that already resolved the mapping continue to use it
    /// safely (compiled mappings are immutable).
    /// </summary>
    bool Unregister(string mappingId);
    
    /// <summary>
    /// Transform <paramref name="source"/> using the named mapping, writing the output directly
    /// into <paramref name="destination"/>. This is the lowest-overhead overload — designed for
    /// HTTP gateways where <paramref name="destination"/> is the response body writer.
    /// </summary>
    void Transform(string mappingId, ReadOnlyMemory<byte> source, IBufferWriter<byte> destination);

    /// <summary>
    /// Same as the memory overload but for non-contiguous buffers (e.g. PipeReader output).
    /// </summary>
    void Transform(string mappingName, ReadOnlySequence<byte> source, IBufferWriter<byte> destination);

    /// <summary>
    /// Run the mapping into a pooled byte array. The caller MUST return the buffer via
    /// <see cref="ArrayPool{T}.Return(T[], bool)"/>; the <paramref name="length"/> out-parameter
    /// indicates the valid portion of the returned buffer.
    /// </summary>
    byte[] TransformToPooled(string mappingId, ReadOnlyMemory<byte> source, out int length);
    
    /// <summary>
    /// Read JSON from <paramref name="source"/>, apply the mapping, and write the output to
    /// <paramref name="destination"/>. Both streams must be readable / writable accordingly;
    /// neither stream is disposed by this call.
    /// </summary>
    void Transform(string mappingId, Stream source, Stream destination);

    /// <summary>Async counterpart of <see cref="Transform(string, Stream, Stream)"/>.</summary>
    ValueTask TransformAsync(
        string mappingId,
        Stream source,
        Stream destination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read JSON from <paramref name="source"/> and return the transformed bytes as a freshly
    /// allocated array. Convenience for callers that don't have an output stream / writer.
    /// </summary>
    byte[] TransformToBytes(string mappingId, Stream source);

    /// <summary>Async counterpart of <see cref="TransformToBytes(string, Stream)"/>.</summary>
    ValueTask<byte[]> TransformToBytesAsync(
        string mappingId,
        Stream source,
        CancellationToken cancellationToken = default);
    
    /// <summary>String convenience. Allocates a result string. Intended for tests and demos.</summary>
    string TransformToString(string mappingId, string sourceJson);
    
    /// <summary>
    /// Iterate the NDJSON chunks in <paramref name="source"/>, apply the same
    /// <paramref name="mappingId"/> mapping to each one, and emit the transformed chunks into
    /// <paramref name="destination"/> using <paramref name="destinationFormat"/> framing.
    /// Optionally appends an SSE <c>[DONE]</c> terminator (no-op for NDJSON output).
    /// </summary>
    void TransformNdjson(
        string mappingId,
        ReadOnlyMemory<byte> source,
        IBufferWriter<byte> destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true);

    /// <summary>
    /// Async, incremental version of <see cref="TransformNdjson"/>. Reads from
    /// <paramref name="source"/> as bytes arrive and flushes each transformed chunk to
    /// <paramref name="destination"/> immediately so downstream consumers see progress.
    /// </summary>
    ValueTask TransformNdjsonAsync(
        string mappingId,
        Stream source,
        Stream destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Iterate the SSE events in <paramref name="source"/>. 
    /// Events whose name is not in the dictionary are silently dropped — this matches the typical "ignore lifecycle
    /// events I don't care about" pattern.
    /// </summary>
    void TransformSse(
        string mappingId,
        ReadOnlyMemory<byte> source,
        IBufferWriter<byte> destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true);

    /// <summary>
    /// Async, incremental version of <see cref="TransformSse"/>. Reads complete SSE events
    /// off the wire, transforms them, and flushes each mapped frame to
    /// <paramref name="destination"/> as soon as it's ready.
    /// </summary>
    ValueTask TransformSseAsync(
        string mappingId,
        IReadOnlyDictionary<string, string> eventRoutes,
        Stream source,
        Stream destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>Writer options applied to the cached <see cref="Utf8JsonWriter"/>.</summary>
    JsonWriterOptions WriterOptions { get; }
}