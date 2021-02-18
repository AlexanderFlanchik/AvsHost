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
    public interface IConversationService
    {
        Task<ConversationModel> GetConversationByAuthorID(string authorId);
        Task<ConversationModel> CreateConversation(ConversationModel conversation);
    }

    public class ConversationService : IConversationService
    {
        private readonly IUserService _userService;
        private readonly IMongoCollection<Conversation> _conversations;

        public ConversationService(MongoEntityRepository entityRepository, IUserService userService)
        {
            _userService = userService;
            _conversations = entityRepository.GetEntityCollection<Conversation>("Conversations");
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
                AuthorID = authorId,
                UnreadMessages = newConversation.UnreadMessages
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
                    AuthorID = authorId, 
                    UnreadMessages = existingConversation.UnreadMessages 
                };
            }

            return null;
        }              
    }
}