using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models.Conversations;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.AdminConversation
{
    public interface IConversationMessagesService
    {
        Task<IEnumerable<ConversationMessageModel>> GetConversationMessagesAsync(string conversationId, int pageNumber, int pageSize);
        Task<ConversationMessageModel> CreateConversationMessage(string authorId, string conversationId, string content);
        Task MakeMessagesRead(IEnumerable<string> messageIds, string userId);
        Task<int> GetUserUnreadMessagesAmount(string authorId);
    }

    public class ConversationMessagesService : IConversationMessagesService
    {
        private readonly IMongoCollection<ConversationMessage> _conversationMessages;
        private readonly IMongoCollection<Conversation> _conversations;

        public ConversationMessagesService(MongoEntityRepository entityRepository)
        {
            _conversationMessages = entityRepository.GetEntityCollection<ConversationMessage>(GeneralConstants.CONVERSATION_MESSAGE_COLLECTION);
            _conversations = entityRepository.GetEntityCollection<Conversation>(GeneralConstants.CONVERSATION_COLLECTION);
        }

        public async Task<IEnumerable<ConversationMessageModel>> GetConversationMessagesAsync(string conversationId, int pageNumber, int pageSize)
        {
            var findOptions = new FindOptions<ConversationMessage>()
            {
                Limit = pageSize,
                Skip = (pageNumber - 1) * pageSize,
                Sort = new SortDefinitionBuilder<ConversationMessage>().Descending("DateAdded")
            };

            var conversationsMessagesQuery = await _conversationMessages.FindAsync(
                    new FilterDefinitionBuilder<ConversationMessage>().Where(c => c.ConversationID == conversationId),
                    findOptions
                ).ConfigureAwait(false);

            return (await conversationsMessagesQuery.ToListAsync())
                    .Select(m => new ConversationMessageModel
                    {
                        Id = m.Id,
                        Content = m.Content,
                        AuthorID = m.UserID,
                        DateAdded = m.DateAdded,
                        ConversationId = m.ConversationID,
                        ViewedBy = m.ViewedBy.ToArray()
                    });
        }

        public async Task<ConversationMessageModel> CreateConversationMessage(string authorId, string conversationId, string content)
        {
            var cf = new FilterDefinitionBuilder<Conversation>().Where(c => c.Id == conversationId);
            if (!(await _conversations.FindAsync(cf)).Any())
            {
                return null; // This is a bad request with invalid conversation ID
            }

            var messageDate = DateTime.UtcNow;
            var newConversationMessage = new ConversationMessage 
                { 
                    Content = content, 
                    ConversationID = conversationId, 
                    UserID = authorId, 
                    DateAdded = messageDate 
                };

            newConversationMessage.ViewedBy.Add(authorId);
            await _conversationMessages.InsertOneAsync(newConversationMessage).ConfigureAwait(false);          

            return new ConversationMessageModel 
            { 
                Id = newConversationMessage.Id, 
                AuthorID = authorId, 
                Content = content, 
                DateAdded = newConversationMessage.DateAdded,
                ConversationId = newConversationMessage.ConversationID
            };
        }

        public async Task MakeMessagesRead(IEnumerable<string> messageIds, string userId)
        {
            var filter = new FilterDefinitionBuilder<ConversationMessage>().In(m => m.Id, messageIds);
            var update = new UpdateDefinitionBuilder<ConversationMessage>().PushEach(v => v.ViewedBy, new[] { userId });
            
            await _conversationMessages.UpdateManyAsync(filter, update).ConfigureAwait(false);                       
        }

        public async Task<int> GetUserUnreadMessagesAmount(string authorId)
        {
            var conversation = (await _conversations.FindAsync(new FilterDefinitionBuilder<Conversation>().Where(c => c.AuthorID == authorId))
                ).FirstOrDefault();

            if (conversation == null)
            {
                return 0;
            }

            var filter = Builders<ConversationMessage>.Filter.Where(m => m.ConversationID == conversation.Id && !m.ViewedBy.Contains(authorId));
            var query = await _conversationMessages.FindAsync(filter);
            
            return (await query.ToListAsync()).Count;
        }
    }
}