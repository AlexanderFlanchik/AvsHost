using System.Buffers;
using System.Text;
using System.Text.Json;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;
using Xunit.Abstractions;

namespace Avs.StaticSiteHosting.Tests.GenAiApi.PayloadFormatters;

public class JsonRemapperTests : JsonRemapperTestsBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    public JsonRemapperTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task OpenAiRequest_MapsToAnthropicRequest()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "openai-to-anthropic-request.json");
        remapper.RegisterFromJson(mappingJson, "openai-to-anthropic-request");

        var sourceJson = await GetContent("Samples", "openai-request.json");
        var destination = new ArrayBufferWriter<byte>();

        remapper.Transform("openai-to-anthropic-request", Encoding.UTF8.GetBytes(sourceJson), destination);
        using var document = JsonDocument.Parse(destination.WrittenMemory);

        var root = document.RootElement;
        Assert.Equal("gpt-4.1", root.GetProperty("model").GetString());
        Assert.Equal(100, root.GetProperty("max_tokens").GetInt32());
        Assert.Equal(0.7, root.GetProperty("temperature").GetDouble());

        var systemProperty = root.GetProperty("system");
        Assert.Contains("You are a helpful assistant.", systemProperty.GetRawText());

        var messages = root.GetProperty("messages").EnumerateArray().ToArray();
        Assert.Equal(2, messages.Length);
        Assert.Equal("user", messages[0].GetProperty("role").GetString());
        Assert.Equal("Write a short poem.", messages[0].GetProperty("content")[0].GetProperty("text").GetString());
        Assert.Equal("assistant", messages[1].GetProperty("role").GetString());
        Assert.Equal("Certainly!", messages[1].GetProperty("content")[0].GetProperty("text").GetString());
    }

    [Fact]
    public async Task OpenAiRequest_MapsToGeminiRequest()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "openai-to-gemini-request.json");
        remapper.RegisterFromJson(mappingJson, "openai-to-gemini-request");

        var sourceJson = await GetContent("Samples", "openai-request.json");
        await using var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(sourceJson));
        await using var destinationStream = new MemoryStream();

        await remapper.TransformAsync("openai-to-gemini-request", sourceStream, destinationStream);
        var actual = Encoding.UTF8.GetString(destinationStream.ToArray());
        using var document = JsonDocument.Parse(actual);

        var root = document.RootElement;
        Assert.Equal("gpt-4.1", root.GetProperty("model").GetString());
        Assert.Equal("system", root.GetProperty("systemInstruction").GetProperty("role").GetString());
        Assert.Equal("You are a helpful assistant.", root.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString());
        Assert.Equal("user", root.GetProperty("contents")[0].GetProperty("role").GetString());
        Assert.Equal("model", root.GetProperty("contents")[1].GetProperty("role").GetString());
        Assert.Equal(0.7, root.GetProperty("generationConfig").GetProperty("temperature").GetDouble());
        Assert.Equal(100, root.GetProperty("generationConfig").GetProperty("maxOutputTokens").GetInt32());
    }

    [Fact]
    public async Task AnthropicRequest_MapsToOpenAiRequest()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "antropic-to-openai-request.json");
        remapper.RegisterFromJson(mappingJson, "antropic-to-openai-request");

        var sourceJson = await GetContent("Samples", "anthropic-request.json");
        var actual = remapper.TransformToString("antropic-to-openai-request", sourceJson);

        using var document = JsonDocument.Parse(actual);
        var root = document.RootElement;

        Assert.Equal("claude-3", root.GetProperty("model").GetString());
        var messages = root.GetProperty("messages").EnumerateArray().ToArray();
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("Assistant should be concise.", messages[0].GetProperty("content").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("Summarize this.", messages[1].GetProperty("content").GetString());
    }

    [Fact]
    public async Task AnthropicResponse_MapsToOpenAiResponse()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "antropic-to-openai-response.json");
        remapper.RegisterFromJson(mappingJson, "antropic-to-openai-response");

        var sourceJson = await GetContent("Samples", "anthropic-response.json");
        var destination = new ArrayBufferWriter<byte>();

        remapper.Transform("antropic-to-openai-response", Encoding.UTF8.GetBytes(sourceJson), destination);
        using var document = JsonDocument.Parse(destination.WrittenMemory);

        var root = document.RootElement;
        Assert.Equal("anthropic-123", root.GetProperty("id").GetString());
        Assert.Equal("chat.completion", root.GetProperty("object").GetString());
        Assert.Equal("claude-3", root.GetProperty("model").GetString());

        var choice = root.GetProperty("choices")[0];
        Assert.Equal("assistant", choice.GetProperty("message").GetProperty("role").GetString());
        Assert.Equal("Hello from Anthropic.", choice.GetProperty("message").GetProperty("content").GetString());
        Assert.Equal("stop", choice.GetProperty("finish_reason").GetString());

        var usage = root.GetProperty("usage");
        Assert.Equal(12, usage.GetProperty("prompt_tokens").GetInt32());
        Assert.Equal(34, usage.GetProperty("completion_tokens").GetInt32());
        Assert.Equal(46, usage.GetProperty("total_tokens").GetInt32());
    }

    [Fact]
    public async Task GeminiResponse_MapsToOpenAiResponse()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "gemini-to-openai-response.json");
        remapper.RegisterFromJson(mappingJson, "gemini-to-openai-response");

        var sourceJson = await GetContent("Samples", "gemini-response.json");
        await using var sourceStream = new MemoryStream(Encoding.UTF8.GetBytes(sourceJson));
        var actualBytes = await remapper.TransformToBytesAsync("gemini-to-openai-response", sourceStream);
        using var document = JsonDocument.Parse(actualBytes);

        var root = document.RootElement;
        Assert.Equal("chat.completion", root.GetProperty("object").GetString());
        Assert.Equal("gemini-1.0", root.GetProperty("model").GetString());

        var choice = root.GetProperty("choices")[0];
        Assert.Equal("assistant", choice.GetProperty("message").GetProperty("role").GetString());
        Assert.Equal("Gemini says hi.", choice.GetProperty("message").GetProperty("content").GetString());
        Assert.Equal("stop", choice.GetProperty("finish_reason").GetString());

        var usage = root.GetProperty("usage");
        Assert.Equal(5, usage.GetProperty("prompt_tokens").GetInt32());
        Assert.Equal(10, usage.GetProperty("completion_tokens").GetInt32());
        Assert.Equal(15, usage.GetProperty("total_tokens").GetInt32());
    }

    [Fact]
    public async Task OllamaResponse_MapsToOpenAiResponse()
    {
        var remapper = new JsonRemapper();
        var mappingJson = await GetContent("Mappings", "ollama-to-openai-response.json");
        remapper.RegisterFromJson(mappingJson, "ollama-to-openai-response");

        var sourceJson = await GetContent("Samples", "ollama-response.json");
        var actual = remapper.TransformToString("ollama-to-openai-response", sourceJson);

        using var document = JsonDocument.Parse(actual);
        var root = document.RootElement;
        Assert.Equal("chat.completion", root.GetProperty("object").GetString());
        Assert.Equal("llama3.2", root.GetProperty("model").GetString());

        var choice = root.GetProperty("choices")[0];
        Assert.Equal(0, choice.GetProperty("index").GetInt32());
        Assert.Equal("assistant", choice.GetProperty("message").GetProperty("role").GetString());
        Assert.Equal("Backpropagation is the algorithm that computes gradients of a loss with respect to network weights by applying the chain rule layer-by-layer in reverse.",
            choice.GetProperty("message").GetProperty("content").GetString());
        Assert.Equal("stop", choice.GetProperty("finish_reason").GetString());
    }
}
