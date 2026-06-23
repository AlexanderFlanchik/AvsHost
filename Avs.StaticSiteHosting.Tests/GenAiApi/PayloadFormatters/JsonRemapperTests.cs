using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    public async Task RegisterFromJson_AndLookupByName_RoundTrips()
    {
        // Arrange
        const string mappingId = "trivial";
        string mapping = await GetContent("Mappings", "trivial-mapping.json");
        var remapper = new JsonRemapper();

        // Act
        var compiled = remapper.RegisterFromJson(mapping, mappingId);

        // Assert
        Assert.Equal("trivial", compiled.Id);
        Assert.True(remapper.TryGet(mappingId, out var resolved));
        Assert.Same(compiled, resolved);
        Assert.Same(compiled, remapper.Get(mappingId));
    }
    
    [Fact]
    public void Get_UnknownMapping_Throws()
    { 
        // Arrange
        var remapper = new JsonRemapper();

        // Act + Assert
        Assert.False(remapper.TryGet("does-not-exist", out _));
        Assert.Throws<KeyNotFoundException>(() => remapper.Get("does-not-exist"));
    }
  
    [Fact]
    public async Task Transform_ShouldCorrectlyTransform_OllamaToOpenAiResponse()
    {
        // Arrange
        string mapping = await GetContent("Mappings","ollama-to-openai-response.json");
        string ollamaResponseJson = await GetContent("Samples", "ollama-response.json");
        
        // Act
        var root = await ActAndGetResponse(mapping, ollamaResponseJson);

        // Assert
        AssertOllamaResponse(root);
    }

    [Fact]
    public async Task Transform_ShouldCorrectlyTransform_OllamaToOpenAiResponseWithTools()
    {
        // Arrange
        string mapping = await GetContent("Mappings","ollama-to-openai-response.json");
        string ollamaResponseJson = await GetContent("Samples", "ollama-response-with-tools.json");
        
        // Act
        var root = await ActAndGetResponse(mapping, ollamaResponseJson);
        
        // Assert
        AssertOllamaResponseWithToolCall(root);
    }
    
    [Fact]
    public async Task TransformAsync_ShouldCorrectlyTransform_OllamaToOpenAiResponseWithTools()
    {
        // Arrange
        string mapping = await GetContent("Mappings","ollama-to-openai-response.json");
        string ollamaResponseJson = await GetContent("Samples", "ollama-response-with-tools.json");
        
        // Act
        var root = await ActAndGetResponse(mapping, ollamaResponseJson, true);
        
        // Assert
        AssertOllamaResponseWithToolCall(root);
    }
    
    [Fact]
    public async Task TransformAsync_ShouldCorrectlyTransform_OllamaToOpenAiResponse()
    {
        // Arrange
        string mapping = await GetContent("Mappings","ollama-to-openai-response.json");
        string ollamaResponseJson = await GetContent("Samples", "ollama-response.json");
        
        // Act
        var root = await ActAndGetResponse(mapping, ollamaResponseJson, true);

        // Assert
        AssertOllamaResponse(root);
    }
    
    private async Task<JsonElement> ActAndGetResponse(string mapping, string input, bool isAsync = false)
    {
        const string mappingId = "test_mapping";         
        var remapper = new JsonRemapper();
        
        remapper.RegisterFromJson(mapping, mappingId);                                                
        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(input));        
        
        using var outputStream = new MemoryStream();                                  
        var sw = new Stopwatch();                                                     
        sw.Start();
        
        if (isAsync)
        {
            await remapper.TransformAsync(mappingId, inputStream, outputStream);
        }
        else
        {
            remapper.Transform(mappingId, inputStream, outputStream);                     
        }
        
        sw.Stop();                                                                    
        _testOutputHelper.WriteLine($"JSON remap time: {sw.Elapsed.Microseconds} us");
        
        outputStream.Position = 0;                                       
        using var outputDataDocument = JsonDocument.Parse(outputStream); 
        var root = outputDataDocument.RootElement;                       
        
        return root.Clone();
    }
    
    #region Assert Methods
    
    private static void AssertOllamaResponse(JsonElement root)
    {
        Assert.Equal("chat.completion", root.GetProperty("object").GetString());
        Assert.Equal("llama3.2", root.GetProperty("model").GetString());
        Assert.Equal(1, root.GetProperty("choices").GetArrayLength());
        
        var choice = JsonNode.Parse(root.GetProperty("choices").GetRawText())?[0]!;
        Assert.Equal(0, choice["index"]!.GetValue<int>());
        Assert.Equal("assistant", choice["message"]!.AsObject()["role"]!.GetValue<string>());
        Assert.Contains("Backpropagation is the algorithm", choice["message"]!.AsObject()["content"]!.GetValue<string>());
        Assert.Equal("stop", choice["finish_reason"]!.GetValue<string>());

        var usage = JsonNode.Parse(root.GetProperty("usage").GetRawText())!;
        Assert.Equal(23, usage.AsObject()["prompt_tokens"]!.GetValue<int>());
        Assert.Equal(56, usage.AsObject()["completion_tokens"]!.GetValue<int>());
        Assert.Equal(79, usage.AsObject()["total_tokens"]!.GetValue<int>());
    }
    
    private static void AssertOllamaResponseWithToolCall(JsonElement root)
    {
        Assert.Equal("chat.completion", root.GetProperty("object").GetString());
        Assert.Equal("llama3.2", root.GetProperty("model").GetString());
        Assert.Equal(1, root.GetProperty("choices").GetArrayLength());
        
        var choice = JsonNode.Parse(root.GetProperty("choices").GetRawText())?[0]!;
        Assert.Equal(0, choice["index"]!.GetValue<int>());
        Assert.Equal("assistant", choice["message"]!.AsObject()["role"]!.GetValue<string>());

        var toolCalls = choice["message"]!.AsObject()["tool_calls"]!.AsArray();
        Assert.Single(toolCalls);
        
        var toolCall = toolCalls[0]!;
        Assert.Equal("function", toolCall["type"]!.GetValue<string>());
        Assert.Equal("get_current_weather", toolCall["function"]!.AsObject()["name"]!.GetValue<string>());
        var args = JsonNode.Parse(toolCall["function"]!.AsObject()["arguments"]!.GetValue<string>())!;
        Assert.Equal("Paris", args["city"]!.GetValue<string>());
        Assert.Equal("celsius", args["unit"]!.GetValue<string>());

        Assert.Equal("stop", choice["finish_reason"]!.GetValue<string>());

        var usage = JsonNode.Parse(root.GetProperty("usage").GetRawText())!;
        Assert.Equal(35, usage.AsObject()["prompt_tokens"]!.GetValue<int>());
        Assert.Equal(22, usage.AsObject()["completion_tokens"]!.GetValue<int>());
        Assert.Equal(57, usage.AsObject()["total_tokens"]!.GetValue<int>());
    }
    
    #endregion
}