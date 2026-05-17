using Avs.StaticSiteHosting.GenAiApi.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Avs.StaticSiteHosting.GenAiApi.Models;

public class PayloadMapping
{
    /// <summary>
    /// Payload mapping ID
    /// </summary>
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Vendor code
    /// </summary>
    public string VendorCode { get; init; } = null!;

    /// <summary>
    /// Mapping content
    /// </summary>
    public string JsonContent { get; set; } = null!;
    
    /// <summary>
    /// Mapping type (for request, response, or response stream)
    /// </summary>
    public PayloadMappingType Type { get; set; }
}