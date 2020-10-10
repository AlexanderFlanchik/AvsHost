using System.Collections.Generic;

namespace Avs.StaticSiteHosting.DTOs
{
    public class HelpTopicModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int OrdinalNo { get; set; }
        public List<TopicParagraphModel> Paragraphs { get; set; }
        public HelpTopicModel()
        {
            Paragraphs = new List<TopicParagraphModel>();
        }

        public string[] RolesAllowed { get; set; }
    }
}