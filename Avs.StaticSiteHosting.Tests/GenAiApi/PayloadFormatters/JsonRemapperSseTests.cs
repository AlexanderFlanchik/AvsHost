using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;
using Xunit;

namespace Avs.StaticSiteHosting.Tests.GenAiApi.PayloadFormatters;

public class JsonRemapperSseTests : JsonRemapperTestsBase
{
    [Fact]
    public async Task TransformSse_TransformsGeminiSseStreamSynchronously()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "gemini-chunk-to-openai-chunk.json");
        remapper.RegisterFromJson(mappingJson, "gemini-chunk-to-openai-chunk");

        var source = await GetContent("Samples", "gemini-sse-stream.txt");
        var destination = new ArrayBufferWriter<byte>();

        remapper.TransformSse("gemini-chunk-to-openai-chunk", Encoding.UTF8.GetBytes(source), destination);
        var actual = Encoding.UTF8.GetString(destination.WrittenSpan);

        Assert.Contains("data: [DONE]", actual);
        var firstFrame = ParseSseData(actual, 0);
        Assert.Equal("assistant", firstFrame.GetProperty("choices")[0].GetProperty("delta").GetProperty("role").GetString());
        Assert.Equal("Streamed chunk", firstFrame.GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString());
    }

    [Fact]
    public async Task TransformSseAsync_TransformsGeminiSseStreamAndAppendsDoneTerminator()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "gemini-chunk-to-openai-chunk.json");
        remapper.RegisterFromJson(mappingJson, "gemini-chunk-to-openai-chunk");

        var source = await GetContent("Samples", "gemini-sse-stream.txt");
        await using var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(source));
        await using var destinationStream = new MemoryStream();

        await remapper.TransformSseAsync(
            "gemini-chunk-to-openai-chunk",
            new Dictionary<string, string> { ["message"] = "message" },
            sourceStream,
            destinationStream);

        var actual = Encoding.UTF8.GetString(destinationStream.ToArray());
        Assert.Contains("data: [DONE]", actual);
        var firstFrame = ParseSseData(actual, 0);
        Assert.Equal("assistant", firstFrame.GetProperty("choices")[0].GetProperty("delta").GetProperty("role").GetString());
        Assert.Equal("Streamed chunk", firstFrame.GetProperty("choices")[0].GetProperty("delta").GetProperty("content").GetString());
    }

    private static JsonElement ParseSseData(string sseStream, int frameIndex)
    {
        var frames = sseStream.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        var dataFrames = new List<string>();
        foreach (var frame in frames)
        {
            var trimmed = frame.Trim();
            if (trimmed.StartsWith("data: ") && !trimmed.StartsWith("data: [DONE]", StringComparison.Ordinal))
            {
                dataFrames.Add(trimmed[6..]);
            }
        }

        return JsonDocument.Parse(dataFrames[frameIndex]).RootElement;
    }
}
