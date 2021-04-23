using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Linq;

namespace Avs.StaticSiteHosting.Web.Models
{
    public class SiteEvent : BaseEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string SiteId { get; set; }
        public SiteEventType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
        public Site[] Sites { get; set; }   // navigation property     
    }
}