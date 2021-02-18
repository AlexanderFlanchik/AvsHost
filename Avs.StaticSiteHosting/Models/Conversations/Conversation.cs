using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Avs.StaticSiteHosting.Web.Models.Conversations
{
    public class Conversation : BaseEntity
    {
        public int UnreadMessages { get; set; }
        public string AuthorID { get; set; }
    }

    public class ConversationMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ConversationID { get; set; }
        public string UserID { get; set; }
        public DateTime DateAdded { get; set; }
        public bool Viewed { get; set; }
        public string Content { get; set; }
    }
}