using Avs.StaticSiteHosting.GenAiApi.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Avs.StaticSiteHosting.GenAiApi.Models;

public class Model
{
    /// <summary>
    /// Model ID in database
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Model name
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Model vendor ID
    /// </summary>
    [BsonRepresentation(BsonType.ObjectId)]
    public string VendorId { get; set; } = null!;

    /// <summary>
    /// Model request paths (e.g. /chat/completions, /chat, ...)
    /// </summary>
    public string[] Paths { get; set; } = [];
    
    /// <summary>
    /// Checks if model is available (not deprecated, etc.)
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Custom model settings in JSON format (optional)
    /// </summary>
    public string? CustomSettings { get; set; }

    /// <summary>
    /// LLM features that are supported by the model
    /// </summary>
    public LlmFeature[] Features { get; set; } = [];
    
    /// <summary>
    /// Payload mappings for model (key - mapping type, value - mapping ID)
    /// </summary>
    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
    public IDictionary<PayloadMappingType, string> PayloadMappings { get; set; } = new Dictionary<PayloadMappingType, string>();
}