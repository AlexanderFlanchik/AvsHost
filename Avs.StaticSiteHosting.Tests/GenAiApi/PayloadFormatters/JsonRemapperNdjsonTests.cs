using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;
using Xunit;

namespace Avs.StaticSiteHosting.Tests.GenAiApi.PayloadFormatters;

public class JsonRemapperNdjsonTests : JsonRemapperTestsBase
{
    [Fact]
    public async Task TransformNdjson_NewlineDelimitedJson_TransformsOllamaChunks()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "ollama-chunk-to-openai-chunk.json");
        remapper.RegisterFromJson(mappingJson, "ollama-chunk-to-openai-chunk");

        var source = await GetContent("Samples", "ollama-ndjson.jsonl");
        var destination = new ArrayBufferWriter<byte>();

        remapper.TransformNdjson(
            "ollama-chunk-to-openai-chunk",
            Encoding.UTF8.GetBytes(source),
            destination,
            JsonStreamFormat.NewlineDelimitedJson);

        var actual = Encoding.UTF8.GetString(destination.WrittenSpan);
        var lines = actual.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);

        using var first = JsonDocument.Parse(lines[0]);
        Assert.Equal("First partial", first.RootElement.GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString());

        using var second = JsonDocument.Parse(lines[1]);
        Assert.Equal("stop", second.RootElement.GetProperty("choices")[0].GetProperty("finish_reason").GetString());
    }

    [Fact]
    public async Task TransformNdjsonAsync_ServerSentEvents_TransformsOllamaChunksAndAppendsDone()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "ollama-chunk-to-openai-chunk.json");
        remapper.RegisterFromJson(mappingJson, "ollama-chunk-to-openai-chunk");

        var source = await GetContent("Samples", "ollama-ndjson.jsonl");
        await using var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(source));
        await using var destinationStream = new MemoryStream();

        await remapper.TransformNdjsonAsync(
            "ollama-chunk-to-openai-chunk",
            sourceStream,
            destinationStream,
            JsonStreamFormat.ServerSentEvents);

        var actual = Encoding.UTF8.GetString(destinationStream.ToArray());
        Assert.Contains("data: [DONE]", actual);

        var frames = ParseSseDataFrames(actual);
        Assert.Equal(2, frames.Count);
        Assert.Equal("First partial", frames[0].GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString());
        Assert.Equal("stop", frames[1].GetProperty("choices")[0].GetProperty("finish_reason").GetString());
    }

    private static IReadOnlyList<JsonElement> ParseSseDataFrames(string sseStream)
    {
        var frames = sseStream.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var dataFrames = new List<JsonElement>();

        foreach (var frame in frames)
        {
            var trimmed = frame.Trim();
            if (trimmed.StartsWith("data: ") && !trimmed.StartsWith("data: [DONE]", StringComparison.Ordinal))
            {
                dataFrames.Add(JsonDocument.Parse(trimmed[6..]).RootElement.Clone());
            }
        }

        return dataFrames;
    }
}
