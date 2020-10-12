using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web;
using Microsoft.AspNetCore.StaticFiles;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Avs.StaticSiteHosting.DataMigrator
{
    public static class HelpContentInitializer
    {
        private const string SECTION_DATA_FILE = "helpsections.json";
        private const string TOPIC_DATA_FILE = "helptopics.json";
        private const string PARAGRAPH_DATA_FILE = "paragraphs.json";

        public static async Task InitHelpData(MongoEntityRepository entityRepository)
        {
            await ClearHelpData(entityRepository);

            var contentPath = Path.Combine(Directory.GetCurrentDirectory(), "Content");
            var sections = JsonConvert.DeserializeObject<List<Section>>(await File.ReadAllTextAsync(Path.Combine(contentPath, SECTION_DATA_FILE)));
            var topics = JsonConvert.DeserializeObject<List<Topic>>(await File.ReadAllTextAsync(Path.Combine(contentPath, TOPIC_DATA_FILE)));
            var paragraphs = JsonConvert.DeserializeObject<List<Paragraph>>(await File.ReadAllTextAsync(Path.Combine(contentPath, PARAGRAPH_DATA_FILE)));

            var sectionsCollection = entityRepository.GetEntityCollection<HelpSection>(GeneralConstants.HELPSECTION_COLLECTION);
            var topicCollection = entityRepository.GetEntityCollection<HelpTopic>(GeneralConstants.HELPTOPIC_COLLECTION);
            var paragraphCollection = entityRepository.GetEntityCollection<TopicParagraph>(GeneralConstants.TOPICPARAGRAPH_COLLECTION);

            Console.WriteLine("Start processing help sections...");

            foreach (var section in sections)
            {
                var sectionFound = (await sectionsCollection.FindAsync(s => s.ExternalID == section.ExternalID)).FirstOrDefault();
                string parentSectionId = null;
                if (!string.IsNullOrEmpty(section.ParentSectionID))
                {
                    var parentSection = (await sectionsCollection.FindAsync(s => s.ExternalID == section.ParentSectionID)).FirstOrDefault();
                    parentSectionId = parentSection?.Id;
                }

                if (sectionFound != null)
                {                   
                    await sectionsCollection.UpdateOneAsync(
                        new FilterDefinitionBuilder<HelpSection>().Where(s => s.Id == sectionFound.Id),
                        new UpdateDefinitionBuilder<HelpSection>()
                                                        .Set(s => s.Name, section.Name)
                                                        .Set(s => s.ParentSectionId, parentSectionId)
                                                        .Set(s => s.RolesAllowed, section.RolesAllowed));
                }
                else
                {
                    await sectionsCollection.InsertOneAsync(new HelpSection
                                                            {
                                                                ExternalID = section.ExternalID,
                                                                Name = section.Name,
                                                                ParentSectionId = parentSectionId,
                                                                RolesAllowed = section.RolesAllowed
                                                            });
                }
            }

            Console.WriteLine("Processing help sections has been completed. Start processing help topics...");

            foreach (var topic in topics)
            {
                var topicSection = (await sectionsCollection.FindAsync(s => s.ExternalID == topic.SectionExternalID)).FirstOrDefault();
                if (topicSection == null)
                {
                    Console.WriteLine($"Invalid topic {topic.Name}. No section found.");
                    continue;
                }

                var topicFound = (await topicCollection.FindAsync(t => t.SectionId == topicSection.Id && t.OrdinalNo == topic.OrdinalNo)).FirstOrDefault();
                if (topicFound != null)
                {
                    await topicCollection.UpdateOneAsync(new FilterDefinitionBuilder<HelpTopic>().Where(t => t.SectionId == topicSection.Id && t.OrdinalNo == topic.OrdinalNo),
                    new UpdateDefinitionBuilder<HelpTopic>()
                            .Set(t => t.Name, topic.Name)
                            .Set(t => t.OrdinalNo, topic.OrdinalNo)
                            .Set(t => t.RolesAllowed, topic.RolesAllowed)
                            .Set(t => t.SectionId, topicSection.Id)
                    );                   
                }
                else
                {
                    await topicCollection.InsertOneAsync(new HelpTopic 
                                                        { 
                                                            Name = topic.Name,
                                                            OrdinalNo = topic.OrdinalNo,
                                                            SectionId = topicSection.Id,
                                                            RolesAllowed = topic.RolesAllowed
                                                        });
                }
            }

            Console.WriteLine("Processing help topics completed. Start processing topic paragraphs...");

            foreach (var paragraph in paragraphs)
            {
                var topicSection = (await sectionsCollection.FindAsync(s => s.ExternalID == paragraph.SectionExternalID)).FirstOrDefault();
                if (topicSection == null)
                {
                    Console.WriteLine($"Invalid section external ID: {paragraph.SectionExternalID}");
                    continue;
                }

                var paragraphTopic = (await topicCollection.FindAsync(t => t.SectionId == topicSection.Id && t.OrdinalNo == paragraph.TopicOrdinalNo)).FirstOrDefault();
                if (paragraphTopic == null)
                {
                    Console.WriteLine($"No help topic for section with ID = {topicSection.Id} and OrdinalNo = {paragraph.TopicOrdinalNo} found.");
                    continue;
                }

                var paragraphFound = (await paragraphCollection.FindAsync(p => p.TopicId == paragraphTopic.Id && p.OrdinalNo == paragraph.OrdinalNo)).FirstOrDefault();
                if (paragraphFound != null)
                {
                    await paragraphCollection.UpdateOneAsync(new FilterDefinitionBuilder<TopicParagraph>().Where(p => p.Id == paragraphFound.Id),
                        new UpdateDefinitionBuilder<TopicParagraph>()
                                .Set(p => p.Name, paragraph.Name)
                                .Set(p => p.RolesAllowed, paragraph.RolesAllowed)
                                .Set(p => p.Content, paragraph.Content));
                }
                else
                {
                    await paragraphCollection.InsertOneAsync(new TopicParagraph 
                                                            { 
                                                               Name = paragraph.Name,
                                                               OrdinalNo = paragraph.OrdinalNo,
                                                               TopicId = paragraphTopic.Id,
                                                               Content = paragraph.Content,
                                                               RolesAllowed = paragraph.RolesAllowed
                                                            });
                }
            }

            var imageFolder = new DirectoryInfo(Path.Combine(contentPath, "Images\\Help"));
            var imagesToImport = imageFolder.GetFiles();
            var ctpProvider = new FileExtensionContentTypeProvider();
            var helpResources = entityRepository.GetEntityCollection<HelpResource>(GeneralConstants.HELPRESOURCE_COLLECTION);

            Console.WriteLine("Processing help system images...");

            foreach (var img in imagesToImport)
            {
                var resource = (await helpResources.FindAsync(r => r.Name == img.Name)).FirstOrDefault();
                using var ms = new MemoryStream();
                await img.OpenRead().CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);

                if (resource != null)
                {                   
                    await helpResources.UpdateOneAsync(new FilterDefinitionBuilder<HelpResource>().Where(r => r.Name == img.Name),
                        new UpdateDefinitionBuilder<HelpResource>().Set(r => r.Content, ms.ToArray()));
                }
                else
                {
                    var contentType = "application/octet-stream";
                    _ = ctpProvider.TryGetContentType(img.Name, out contentType);
                    await helpResources.InsertOneAsync(new HelpResource
                                                      {
                                                          Name = img.Name,
                                                          Content = ms.ToArray(),
                                                          ContentType = contentType
                                                      });
                }
            }

            Console.WriteLine("Help images have been processed.");
            Console.WriteLine("Help sub-system data initialization has completed successfully.");
        }

        private static async Task ClearHelpData(MongoEntityRepository entityRepository)
        {
            await entityRepository.GetEntityCollection<TopicParagraph>(GeneralConstants.TOPICPARAGRAPH_COLLECTION)
                                  .DeleteManyAsync(new FilterDefinitionBuilder<TopicParagraph>().Empty);

            await entityRepository.GetEntityCollection<HelpTopic>(GeneralConstants.HELPTOPIC_COLLECTION)
                                  .DeleteManyAsync(new FilterDefinitionBuilder<HelpTopic>().Empty);

            await entityRepository.GetEntityCollection<HelpSection>(GeneralConstants.HELPSECTION_COLLECTION)
                                  .DeleteManyAsync(new FilterDefinitionBuilder<HelpSection>().Empty);
        }

        #region Helper classes
        private class Section
        {
            public string Name { get; set; }
            public string ExternalID { get; set; }
            public string ParentSectionID { get; set; }
            public string[] RolesAllowed { get; set; }
        }

        private class Topic
        {
            public int OrdinalNo { get; set; }
            public string SectionName { get; set; }
            public string Name { get; set; }
            public string SectionExternalID { get; set; }
            public string[] RolesAllowed { get; set; }
        }
             

        private class Paragraph
        {
            public string SectionExternalID { get; set; }
            public int TopicOrdinalNo { get; set; }
            public string Name { get; set; }
            public int OrdinalNo { get; set; }
            public string Content { get; set; }
            public string[] RolesAllowed { get; set; }
        }

        #endregion
    }
}