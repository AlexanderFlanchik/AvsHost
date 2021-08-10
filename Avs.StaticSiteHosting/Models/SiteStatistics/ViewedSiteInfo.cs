using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Avs.StaticSiteHosting.Web.Models.SiteStatistics
{
    public class ViewedSiteInfo : BaseEntity
    {
        public ViewedSiteInfo()
        {
            Name = "Viewed";
        }

        public DateTime ViewedTimestamp { get; set; }
        
        [BsonRepresentation(BsonType.ObjectId)]
        public string SiteId { get; set; }
        public string Visitor { get; set; } // The person IP how viewed the site

        public Site[] Sites { get; set; }
    }
}