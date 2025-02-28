using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;

namespace Avs.StaticSiteHosting.Web.Models
{
    public class ContentUpload
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FileName { get; set; }
        public string DestinationPath { get; set; }
        public string UploadSessionId { get; set; }
        public TimeSpan? CacheDuration { get; set; }
    }
}