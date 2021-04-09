using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    /// <summary>
    /// Conversation message DTO.
    /// </summary>
    public class ConversationMessageModel
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public DateTime DateAdded { get; set; }
        public string Content { get; set; }
        public string AuthorID { get; set; }
        public string[] ViewedBy { get; set; }
    }
}