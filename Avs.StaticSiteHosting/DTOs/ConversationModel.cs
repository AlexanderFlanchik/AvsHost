﻿namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ConversationModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int UnreadMessages { get; set; }
        public string AuthorID { get; set; }
    }
}