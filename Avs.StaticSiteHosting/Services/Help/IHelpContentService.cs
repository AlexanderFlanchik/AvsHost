using Avs.StaticSiteHosting.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
{
    public interface IHelpContentService
    {
        Task<int> GetTopicsAmountAsync(string sectionId);
        Task<HelpTopicModel> GetTopicBySectionId(string sectionId, int ordinalNo);
        Task<List<HelpSectionModel>> GetAllHelpSectionsAsync();
        Task<IEnumerable<TopicParagraphModel>> GetTopicContentAsync(string topicId);
        Task<HelpResourceModel> GetHelpResourceAsync(string resourceName);
    }
}