using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;

namespace Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

public sealed class JsonRemapper : IJsonRemapper
{
    private readonly ConcurrentDictionary<string, CompiledMapping> _mappings = new(StringComparer.Ordinal);
    private readonly object _reloadGate = new();

    [ThreadStatic] 
    private static Utf8JsonWriter? _tlsWriter;

    public JsonWriterOptions WriterOptions { get; init; }

    public void Register(CompiledMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        _mappings[mapping.Id] = mapping;
    }

    public CompiledMapping RegisterFromJson(string mappingJson, string mappingId)
    {
        var compiled = MappingCompiler.Compile(mappingJson, mappingId);
        Register(compiled);
        
        return compiled;
    }
    
    public bool TryGet(string mappingId, out CompiledMapping mapping) 
        => _mappings.TryGetValue(mappingId, out mapping!);

    public CompiledMapping Get(string mappingId)
        => _mappings.TryGetValue(mappingId, out var m) ? 
            m : 
            throw new KeyNotFoundException($"Mapping with ID '{mappingId}' not found.");

    public bool Unregister(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return _mappings.TryRemove(name, out _);
    }
    
    public void Transform(string mappingId, ReadOnlyMemory<byte> source, IBufferWriter<byte> destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var mapping = Get(mappingId);
        var writer = GetThreadWriter(destination);
        mapping.Apply(source, destination, writer);
    }

    public void Transform(string mappingId, ReadOnlySequence<byte> source, IBufferWriter<byte> destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var mapping = Get(mappingId);
        var writer = GetThreadWriter(destination);
        mapping.Apply(source, destination, writer);
    }

    public byte[] TransformToPooled(string mappingId, ReadOnlyMemory<byte> source, out int length)
    {
        var mapping = Get(mappingId);
        var bufferWriter = new ArrayPoolBufferWriter(initialCapacity: source.Length + 256);
        try
        {
            var writer = GetThreadWriter(bufferWriter);
            mapping.Apply(source, bufferWriter, writer);
            length = bufferWriter.WrittenCount;
            
            return bufferWriter.DetachBuffer();
        }
        catch
        {
            bufferWriter.Dispose();
            throw;
        }
    }
    
    public void Transform(string mappingId, Stream source, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        var mapping = Get(mappingId);
        using var doc = JsonDocument.Parse(source);
        using var bufferWriter = new ArrayPoolBufferWriter(initialCapacity: 4096);
        
        var writer = GetThreadWriter(bufferWriter);
        mapping.Apply(doc, bufferWriter, writer);
        destination.Write(bufferWriter.WrittenSpan);
    }

    public async ValueTask TransformAsync(
        string mappingId,
        Stream source,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        var mapping = Get(mappingId);
        
        using var doc = await JsonDocument
            .ParseAsync(source, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        using var bufferWriter = new ArrayPoolBufferWriter(initialCapacity: 4096);
        var writer = GetThreadWriter(bufferWriter);
        mapping.Apply(doc, bufferWriter, writer);

        await destination
            .WriteAsync(bufferWriter.WrittenMemory, cancellationToken)
            .ConfigureAwait(false);
    }

    public byte[] TransformToBytes(string mappingId, Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var mapping = Get(mappingId);
        using var doc = JsonDocument.Parse(source);
        using var bufferWriter = new ArrayPoolBufferWriter(initialCapacity: 4096);
        
        var writer = GetThreadWriter(bufferWriter);
        mapping.Apply(doc, bufferWriter, writer);
        
        return bufferWriter.WrittenSpan.ToArray();
    }

    public async ValueTask<byte[]> TransformToBytesAsync(
        string mappingId,
        Stream source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var mapping = Get(mappingId);
        using var doc = await JsonDocument
            .ParseAsync(source, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        using var bufferWriter = new ArrayPoolBufferWriter(initialCapacity: 4096);
        var writer = GetThreadWriter(bufferWriter);
        mapping.Apply(doc, bufferWriter, writer);
        
        return bufferWriter.WrittenSpan.ToArray();
    }
    
    public string TransformToString(string mappingId, string sourceJson)
    {
        var sourceBytes = Encoding.UTF8.GetBytes(sourceJson);
        var rented = TransformToPooled(mappingId, sourceBytes, out int length);
        try
        {
            return Encoding.UTF8.GetString(rented, 0, length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
    
    public void TransformNdjson(
        string mappingId,
        ReadOnlyMemory<byte> source,
        IBufferWriter<byte> destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true)
    {
        ArgumentNullException.ThrowIfNull(destination);
        var mapping = Get(mappingId);

        var perChunk = new ArrayBufferWriter<byte>(256);
        foreach (var chunk in StreamReformatter.ParseNdjson(source))
        {
            perChunk.ResetWrittenCount();
            var writer = GetThreadWriter(perChunk);
            mapping.Apply(chunk, perChunk, writer);
            WriteFrame(destination, perChunk.WrittenSpan, destinationFormat);
        }

        if (writeDoneTerminator && destinationFormat == JsonStreamFormat.ServerSentEvents)
        {
            StreamReformatter.WriteSseDone(destination);
        }
    }

    public async ValueTask TransformNdjsonAsync(
        string mappingId,
        Stream source,
        Stream destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        var mapping = Get(mappingId);

        var reader = PipeReader.Create(source, new StreamPipeReaderOptions(leaveOpen: true));
        var perChunk = new ArrayBufferWriter<byte>(256);
        var frameOut = new ArrayBufferWriter<byte>(256);

        try
        {
            while (true)
            {
                var read = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = read.Buffer;

                while (TryReadNdjsonLine(ref buffer, out var line))
                {
                    if (line.Length == 0) continue;
                    await ProcessChunkAsync(mapping, line, perChunk, frameOut, destination,
                        destinationFormat, cancellationToken).ConfigureAwait(false);
                }

                if (read.IsCompleted)
                {
                    if (buffer.Length > 0)
                    {
                        var trailing = buffer;
                        if (TryStripTrailingCr(ref trailing) && trailing.Length == 0)
                        {
                            // CR-only fragment, ignore.
                        }
                        else if (trailing.Length > 0)
                        {
                            await ProcessChunkAsync(mapping, trailing, perChunk, frameOut, destination,
                                destinationFormat, cancellationToken).ConfigureAwait(false);
                        }
                        buffer = buffer.Slice(buffer.End);
                    }
                    
                    reader.AdvanceTo(buffer.Start, buffer.End);
                    break;
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
            }

            if (writeDoneTerminator && destinationFormat == JsonStreamFormat.ServerSentEvents)
            {
                frameOut.ResetWrittenCount();
                StreamReformatter.WriteSseDone(frameOut);
                
                await destination.WriteAsync(frameOut.WrittenMemory, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }
    
    public void TransformSse(
        string mappingId,
        ReadOnlyMemory<byte> source,
        IBufferWriter<byte> destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var perChunk = new ArrayBufferWriter<byte>(256);
        foreach (var evt in StreamReformatter.ParseSse(source))
        {
            if (evt.IsDone)
            {
                continue;
            }
            
            var mapping = Get(mappingId);
            
            perChunk.ResetWrittenCount();
            var writer = GetThreadWriter(perChunk);
            mapping.Apply(evt.Data, perChunk, writer);
            WriteFrame(destination, perChunk.WrittenSpan, destinationFormat);
        }

        if (writeDoneTerminator && destinationFormat == JsonStreamFormat.ServerSentEvents)
        {
            StreamReformatter.WriteSseDone(destination);
        }
    }

    public async ValueTask TransformSseAsync(
        string mappingId,
        IReadOnlyDictionary<string, string> eventRoutes,
        Stream source,
        Stream destination,
        JsonStreamFormat destinationFormat = JsonStreamFormat.ServerSentEvents,
        bool writeDoneTerminator = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventRoutes);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);

        var reader = PipeReader.Create(source, new StreamPipeReaderOptions(leaveOpen: true));
        var perChunk = new ArrayBufferWriter<byte>(256);
        var frameOut = new ArrayBufferWriter<byte>(256);

        try
        {
            while (true)
            {
                var read = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = read.Buffer;

                while (TryReadSseEvent(ref buffer, out var eventBytes))
                {
                    if (!TryRouteSseEvent(eventBytes, eventRoutes, out var data))
                    {
                        continue;
                    }

                    var mapping = Get(mappingId);
                    await ProcessChunkAsync(mapping, data, perChunk, frameOut, destination,
                        destinationFormat, cancellationToken).ConfigureAwait(false);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);
                if (read.IsCompleted)
                {
                    if (buffer.Length > 0 &&
                        TryRouteSseEvent(buffer, eventRoutes, out var data))
                    {
                        var mapping = Get(mappingId);
                        await ProcessChunkAsync(mapping, data, perChunk, frameOut, destination,
                            destinationFormat, cancellationToken).ConfigureAwait(false);
                    }
                    break;
                }
            }

            if (writeDoneTerminator && destinationFormat == JsonStreamFormat.ServerSentEvents)
            {
                frameOut.ResetWrittenCount();
                StreamReformatter.WriteSseDone(frameOut);
                await destination.WriteAsync(frameOut.WrittenMemory, cancellationToken).ConfigureAwait(false);
                await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }
    
    private async ValueTask ProcessChunkAsync(
        CompiledMapping mapping,
        ReadOnlySequence<byte> chunk,
        ArrayBufferWriter<byte> perChunk,
        ArrayBufferWriter<byte> frameOut,
        Stream destination,
        JsonStreamFormat destinationFormat,
        CancellationToken cancellationToken)
    {
        perChunk.ResetWrittenCount();
        var writer = GetThreadWriter(perChunk);

        if (chunk.IsSingleSegment)
        {
            mapping.Apply(chunk.First, perChunk, writer);
        }
        else
        {
            mapping.Apply(chunk, perChunk, writer);
        }

        frameOut.ResetWrittenCount();
        WriteFrame(frameOut, perChunk.WrittenSpan, destinationFormat);
        
        await destination.WriteAsync(frameOut.WrittenMemory, cancellationToken).ConfigureAwait(false);
        await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void WriteFrame(IBufferWriter<byte> destination, ReadOnlySpan<byte> json, JsonStreamFormat format)
    {
        switch (format)
        {
            case JsonStreamFormat.NewlineDelimitedJson:
                StreamReformatter.WriteNdjsonLine(destination, json);
                break;
            case JsonStreamFormat.ServerSentEvents:
                StreamReformatter.WriteSseFrame(destination, json);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown stream format.");
        }
    }
    
    private static bool TryReadNdjsonLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        var newline = buffer.PositionOf((byte)'\n');
        if (newline is null)
        {
            line = default;
            
            return false;
        }

        line = buffer.Slice(0, newline.Value);
        TryStripTrailingCr(ref line);
        buffer = buffer.Slice(buffer.GetPosition(1, newline.Value));
        
        return true;
    }
    
    private static bool TryReadSseEvent(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> eventBytes)
    {
        var seqReader = new SequenceReader<byte>(buffer);
        while (seqReader.TryReadTo(out ReadOnlySequence<byte> beforeLf, (byte)'\n', advancePastDelimiter: true))
        {
            // Empty line (just \n or \r\n) marks end of event.
            if (beforeLf.IsEmpty)
            {
                eventBytes = buffer.Slice(0, beforeLf.Start);
                buffer = buffer.Slice(seqReader.Position);
                return true;
            }
            
            if (beforeLf.Length == 1 && beforeLf.FirstSpan[0] == (byte)'\r')
            {
                eventBytes = buffer.Slice(0, beforeLf.Start);
                buffer = buffer.Slice(seqReader.Position);
                return true;
            }
        }

        eventBytes = default;
        return false;
    }
    
    private static bool TryRouteSseEvent(
        ReadOnlySequence<byte> eventBytes,
        IReadOnlyDictionary<string, string> eventRoutes,
        out ReadOnlySequence<byte> data)
    {
        ReadOnlySpan<byte> eventPrefix = "event: "u8;
        ReadOnlySpan<byte> dataPrefix  = "data: "u8;
        ReadOnlySpan<byte> doneToken   = "[DONE]"u8;
        
        ReadOnlySequence<byte> dataSeq = default;
        bool hasData = false;

        var seqReader = new SequenceReader<byte>(eventBytes);
        while (seqReader.TryReadTo(out ReadOnlySequence<byte> line, (byte)'\n', advancePastDelimiter: true)
            || (seqReader.Remaining > 0 && CaptureRemaining(ref seqReader, out line)))
        {
            // Trim trailing \r
            TryStripTrailingCr(ref line);
            if (line.IsEmpty)
            {
                continue;
            }

            if (StartsWith(line, dataPrefix))
            {
                dataSeq = line.Slice(dataPrefix.Length);
                hasData = true;
            }
        }

        if (!hasData || IsEqual(dataSeq, doneToken))
        {
            data = default; 
            return false;
        }
        
        data = dataSeq;
        
        return true;
    }

    private static bool CaptureRemaining(ref SequenceReader<byte> reader, out ReadOnlySequence<byte> line)
    {
        line = reader.UnreadSequence;
        reader.AdvanceToEnd();
        return !line.IsEmpty;
    }

    private static bool TryStripTrailingCr(ref ReadOnlySequence<byte> seq)
    {
        if (seq.IsEmpty)
        {
            return false;
        }
        
        var lastSlice = seq.Slice(seq.Length - 1, 1);
        if (lastSlice.FirstSpan[0] == (byte)'\r')
        {
            seq = seq.Slice(0, seq.Length - 1);
            return true;
        }
        return false;
    }

    private static bool StartsWith(ReadOnlySequence<byte> seq, ReadOnlySpan<byte> prefix)
    {
        if (seq.Length < prefix.Length)
        {
            return false;
        }

        if (seq.IsSingleSegment)
        {
            return seq.FirstSpan.StartsWith(prefix);
        }

        var slice = seq.Slice(0, prefix.Length);
        Span<byte> tmp = stackalloc byte[64];
        if (prefix.Length <= tmp.Length)
        {
            slice.CopyTo(tmp);
            return tmp[..prefix.Length].SequenceEqual(prefix);
        }
        
        var rented = ArrayPool<byte>.Shared.Rent(prefix.Length);
        try
        {
            slice.CopyTo(rented);
            
            return rented.AsSpan(0, prefix.Length).SequenceEqual(prefix);
        }
        finally 
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static bool IsEqual(ReadOnlySequence<byte> seq, ReadOnlySpan<byte> value)
    {
        if (seq.Length != value.Length)
        {
            return false;
        }

        if (seq.IsSingleSegment)
        {
            return seq.FirstSpan.SequenceEqual(value);
        }
        
        Span<byte> tmp = value.Length <= 64 ? stackalloc byte[64] : new byte[value.Length];
        seq.CopyTo(tmp);
        
        return tmp[..value.Length].SequenceEqual(value);
    }
    
    private Utf8JsonWriter GetThreadWriter(IBufferWriter<byte> destination)
    {
        var w = _tlsWriter;
        if (w is null)
        {
            w = new Utf8JsonWriter(destination, WriterOptions);
            _tlsWriter = w;
        }
        else
        {
            w.Reset(destination);
        }
        
        return w;
    }
    
    private sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private byte[] _buffer;
        private int _written;

        public ArrayPoolBufferWriter(int initialCapacity)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(Math.Max(256, initialCapacity));
        }

        public int WrittenCount => _written;
        public ReadOnlySpan<byte> WrittenSpan => _buffer.AsSpan(0, _written);
        public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _written);

        public void Advance(int count) => _written += count;

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer.AsMemory(_written);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer.AsSpan(_written);
        }

        private void EnsureCapacity(int sizeHint)
        {
            if (sizeHint < 1)
            {
                sizeHint = 1;
            }
            
            int needed = _written + sizeHint;
            if (needed <= _buffer.Length)
            {
                return;
            }

            int newSize = Math.Max(_buffer.Length * 2, needed);
            var next = ArrayPool<byte>.Shared.Rent(newSize);
            
            Buffer.BlockCopy(_buffer, 0, next, 0, _written);
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = next;
        }

        public byte[] DetachBuffer()
        {
            var b = _buffer;
            _buffer = [];
            
            return b;
        }

        public void Dispose()
        {
            if (_buffer.Length <= 0)
            {
                return;
            }
            
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = Array.Empty<byte>();
        }
    }
}