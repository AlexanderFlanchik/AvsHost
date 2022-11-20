using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Avs.StaticSiteHosting.Web.Models.Tags;

public class Tag : BaseEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; }
    public string BackgroundColor { get; set; }
    public string TextColor { get; set; }
}