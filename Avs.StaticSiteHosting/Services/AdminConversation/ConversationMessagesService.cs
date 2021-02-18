using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models.Conversations;
using Avs.StaticSiteHosting.Web.Services.Identity;
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
    }

    public class ConversationMessagesService : IConversationMessagesService
    {
        private readonly IUserService _userService;
        private readonly IMongoCollection<ConversationMessage> _conversationMessages;

        public ConversationMessagesService(IUserService userService, MongoEntityRepository entityRepository)
        {
            _conversationMessages = entityRepository.GetEntityCollection<ConversationMessage>("ConversationMessages");
            _userService = userService;
        }

        public async Task<IEnumerable<ConversationMessageModel>> GetConversationMessagesAsync(string conversationId, int pageNumber, int pageSize)
        {
            var findOptions = new FindOptions<ConversationMessage>()
            {
                Limit = pageSize,
                Skip = (pageNumber - 1) * pageSize
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
                        DateAdded = m.DateAdded
                    });
        }

        public async Task<ConversationMessageModel> CreateConversationMessage(string authorId, string conversationId, string content)
        {
            var newConversationMessage = new ConversationMessage { Content = content, ConversationID = conversationId, UserID = authorId, DateAdded = DateTime.UtcNow };
            await _conversationMessages.InsertOneAsync(newConversationMessage).ConfigureAwait(false);

            return new ConversationMessageModel 
            { 
                Id = newConversationMessage.Id, 
                AuthorID = authorId, 
                Content = content, 
                DateAdded = newConversationMessage.DateAdded 
            };
        }
    }
}