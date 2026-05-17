using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Avs.StaticSiteHosting.GenAiApi.Models;

public class Vendor
{
    /// <summary>
    /// Vendor ID in database
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// Vendor name (Ollama, Open AI, Azure AI, ...)
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// Vendor code (ollama, openai, azure, ...)
    /// </summary>
    public string Code { get; set; } = null!;
    
    /// <summary>
    /// Vendor API base URL
    /// </summary>
    public string BaseUrl { get; set; } = null!;
    
    /// <summary>
    /// Optional description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Custom settings stored in JSON format
    /// </summary>
    public string? CustomSettings { get; set; }
    
    /// <summary>
    /// Vendor API Key(s), data protected
    /// </summary>
    public string ApiKey { get; set; } = null!;
}