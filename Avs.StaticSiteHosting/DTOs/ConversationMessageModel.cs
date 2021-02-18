using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ConversationMessageModel
    {
        public string Id { get; set; }
        public DateTime DateAdded { get; set; }
        public string Content { get; set; }
        public string AuthorID { get; set; }
    }
}