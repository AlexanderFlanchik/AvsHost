using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Avs.StaticSiteHosting.Web.Models
{
    public class EntityRef
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }
}