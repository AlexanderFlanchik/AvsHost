using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Avs.StaticSiteHosting.Web.Models
{
    /// <summary>
    /// Represents general entity to store in MongoDB database.
    /// </summary>
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}