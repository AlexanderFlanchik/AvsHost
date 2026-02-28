using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Avs.StaticSiteHosting.Web.Models;

public class PagePreviewEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    public string PreviewSessionId { get; set; }
    
    public string HtmlTreeJson { get; set; }
    
    public DateTime Timestamp { get; set; }
}