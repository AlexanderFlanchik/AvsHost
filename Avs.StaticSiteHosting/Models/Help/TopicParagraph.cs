namespace Avs.StaticSiteHosting.Web.Models
{
    public class TopicParagraph: RoleAccessEntity
    {
        public string TopicId { get; set; }
        public string Content { get; set; }
        public int OrdinalNo { get; set; } // paragraph number in the help topic collection
    }
}