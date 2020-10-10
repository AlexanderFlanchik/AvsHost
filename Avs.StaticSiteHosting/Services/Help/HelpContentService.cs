using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
{
    public class HelpContentService : IHelpContentService
    {
        private readonly IMongoCollection<HelpSection> _helpSections;
        private readonly IMongoCollection<HelpTopic> _helpTopics;
        private readonly IMongoCollection<TopicParagraph> _topicParagraphs;

        public HelpContentService(MongoEntityRepository entityRepository)
        {
            _helpSections = entityRepository.GetEntityCollection<HelpSection>(GeneralConstants.HELPSECTION_COLLECTION);
            _helpTopics = entityRepository.GetEntityCollection<HelpTopic>(GeneralConstants.HELPTOPIC_COLLECTION);
            _topicParagraphs = entityRepository.GetEntityCollection<TopicParagraph>(GeneralConstants.TOPICPARAGRAPH_COLLECTION);
        }

        public async Task<List<HelpSectionModel>> GetAllHelpSectionsAsync()
        {
            var helpSectionsCursor = await _helpSections.FindAsync(s => true).ConfigureAwait(false);
            var helpSectionList = await helpSectionsCursor.ToListAsync().ConfigureAwait(false);

            var sectionList = new List<HelpSectionModel>();
            
            var rootSections = helpSectionList.Where(s => string.IsNullOrEmpty(s.ParentSectionId)).ToList();
            var childSections = helpSectionList.Where(s => !string.IsNullOrEmpty(s.ParentSectionId))
                .ToLookup(id => id.ParentSectionId);

            FillHelpSectionList(rootSections, childSections, sectionList);
                       
            return sectionList;
        }

        public async Task<HelpTopicModel> GetTopicBySectionId(string sectionId, int ordinalNo)
        {
            var filter = new FilterDefinitionBuilder<HelpTopic>()
                .Where(t => t.SectionId == sectionId && t.OrdinalNo == ordinalNo);
            
            var topicCursor = await _helpTopics.FindAsync(filter).ConfigureAwait(false);
            var topic = await topicCursor.FirstOrDefaultAsync().ConfigureAwait(false);
            if (topic == null)
            {
                return null;
            }

            return new HelpTopicModel() 
                { 
                    Id = topic.Id, 
                    OrdinalNo = topic.OrdinalNo, 
                    Name = topic.Name,
                    RolesAllowed = topic.RolesAllowed
                };
        }

        public async Task<int> GetTopicsAmountAsync(string sectionId)
        {
            var filter = new FilterDefinitionBuilder<HelpTopic>()
               .Where(t => t.SectionId == sectionId);
            
            var topicCursor = await _helpTopics.FindAsync(filter).ConfigureAwait(false);
            var amount = topicCursor.ToEnumerable().Count();

            return amount;
        }

        public async Task<IEnumerable<TopicParagraphModel>> GetTopicContentAsync(string topicId)
        {
            var filter = new FilterDefinitionBuilder<TopicParagraph>()
                    .Where(p => p.TopicId == topicId);
            var pgCursor = await _topicParagraphs.FindAsync(filter).ConfigureAwait(false);
            var paragraphList = await pgCursor.ToListAsync().ConfigureAwait(false);

            return paragraphList.Select(p => new TopicParagraphModel { Id = p.Id, Content = p.Content, RolesAllowed = p.RolesAllowed });
        }

        private void FillHelpSectionList(IEnumerable<HelpSection> rootSectionList, ILookup<string, HelpSection> childSections, List<HelpSectionModel> resultSectionList)
        {
            foreach (var sec in rootSectionList)
            {
                var helpSectionModel = new HelpSectionModel() 
                    { 
                        Id = sec.Id, 
                        Name = sec.Name,
                        RolesAllowed = sec.RolesAllowed
                    };

                var children = childSections[sec.Id];
                if (children != null && children.Any())
                {
                    FillHelpSectionList(children, childSections, helpSectionModel.Sections);
                }

                resultSectionList.Add(helpSectionModel);
            }
        }
    }
}