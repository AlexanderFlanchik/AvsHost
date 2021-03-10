using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models.Conversations;
using Avs.StaticSiteHosting.Web.Services.Identity;

namespace Avs.StaticSiteHosting.Web.Services.AdminConversation
{
    public interface IConversationService
    {
        Task<IEnumerable<ConversationModel>> GetLatestConversations(int pageNumber, int pageSize, string currentUserId);
        Task<ConversationModel> GetConversationByAuthorID(string authorId);
        Task<ConversationModel> CreateConversation(ConversationModel conversation);
    }

    public class ConversationService : IConversationService
    {
        private readonly IUserService _userService;
        private readonly IMongoCollection<Conversation> _conversations;
        private readonly IMongoCollection<ConversationMessage> _conversationMessages;
        public ConversationService(MongoEntityRepository entityRepository, IUserService userService)
        {
            _userService = userService;
            _conversations = entityRepository.GetEntityCollection<Conversation>("Conversations");
            _conversationMessages = entityRepository.GetEntityCollection<ConversationMessage>("ConversationMessages");
        }

        public async Task<IEnumerable<ConversationModel>> GetLatestConversations(int pageNumber, int pageSize, string currentUserId)
        {
            var filter = Builders<ConversationMessage>.Filter.Where(c => !c.ViewedBy.Contains(currentUserId));
            var query = _conversationMessages.Aggregate()
                .Match(filter)
                .SortByDescending(d => d.DateAdded)
                .Group(k => k.ConversationID, g => new { ConversationId = g.Key, UnreadMessages = g.Count() })
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize);

            var unreadConversations = await query.ToListAsync();
            var convIds = unreadConversations.Select(id => id.ConversationId).ToArray();

            var cf = Builders<Conversation>.Filter.In(f => f.Id, convIds);
            var conversationsCursor = await _conversations.FindAsync(cf).ConfigureAwait(false);
            var conversationsFound = await conversationsCursor.ToListAsync();

            var results = from uc in unreadConversations
                          join c in conversationsFound on uc.ConversationId equals c.Id
                          select new ConversationModel { Id = c.Id, Name = c.Name, AuthorID = c.AuthorID, UnreadMessages = uc.UnreadMessages };
           
            return results;
        }

        public async Task<ConversationModel> CreateConversation(ConversationModel conversation)
        {
            var authorId = conversation.AuthorID;
            var existingConversation = (await _conversations.FindAsync(
                    new FilterDefinitionBuilder<Conversation>().Where(c => c.AuthorID == authorId)).ConfigureAwait(false)
                   ).FirstOrDefault();

            if (existingConversation != null)
            {
                throw new InvalidOperationException("Conversation already exists.");
            }

            var author = await _userService.GetUserByIdAsync(authorId);
            var newConversation = new Conversation { AuthorID = authorId, Name = author.Name };

            await _conversations.InsertOneAsync(newConversation).ConfigureAwait(false);

            return new ConversationModel
            {
                Id = newConversation.Id,
                Name = newConversation.Name,
                AuthorID = authorId               
            };
        }

        public async Task<ConversationModel> GetConversationByAuthorID(string authorId)
        {
            var existingConversation = (await _conversations.FindAsync(
                    new FilterDefinitionBuilder<Conversation>().Where(c => c.AuthorID == authorId)).ConfigureAwait(false)
                   ).FirstOrDefault();

            if (existingConversation != null)
            {
                return new ConversationModel 
                { 
                    Id = existingConversation.Id, 
                    Name = existingConversation.Name, 
                    AuthorID = authorId
                };
            }

            return null;
        }              
    }
}