﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models.Conversations;
using Avs.StaticSiteHosting.Web.Services.Identity;
using System.Linq.Expressions;

namespace Avs.StaticSiteHosting.Web.Services.AdminConversation
{
    public interface IConversationService
    {
        /// <summary>
        /// Gets a list with conversations that contain unread messages for user with ID = currentUserId
        /// </summary>
        /// <param name="pageNumber">Page</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="currentUserId">Current user ID</param>
        /// <returns>total conversation amount and paged conversation list.</returns>
        Task<(long, IEnumerable<ConversationModel>)> GetLatestConversations(int pageNumber, int pageSize, string currentUserId);
        
        /// <summary>
        /// Gets conversation entity by its ID.
        /// </summary>
        /// <param name="conversationId">Conversation ID</param>
        /// <returns></returns>
        Task<ConversationModel> GetConversationById(string conversationId);
        
        /// <summary>
        /// Gets conversation by its author's ID.
        /// </summary>
        /// <param name="authorId">Author's ID</param>
        /// <returns></returns>               
        Task<ConversationModel> GetConversationByAuthorID(string authorId);

        /// <summary>
        /// Creates a new user-admin conversation.
        /// </summary>
        /// <param name="conversation">Conversation data</param>
        /// <returns></returns>
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

        public async Task<(long, IEnumerable<ConversationModel>)> GetLatestConversations(int pageNumber, int pageSize, string currentUserId)
        {
            var filter = Builders<ConversationMessage>.Filter.Where(c => !c.ViewedBy.Contains(currentUserId));
            var aggr = _conversationMessages.Aggregate()
                .Match(filter)
                .Group(k => k.ConversationID, g => new { ConversationId = g.Key, LastUpdated = g.Max(m => m.DateAdded), UnreadMessages = g.Count() });

            var total = (await aggr.Count().FirstAsync())?.Count ?? 0;
            var query = aggr.Skip((pageNumber - 1) * pageSize).Limit(pageSize);

            var unreadConversations = await query.ToListAsync();
            var convIds = unreadConversations.Select(id => id.ConversationId).ToArray();

            var cf = Builders<Conversation>.Filter.In(f => f.Id, convIds);
            var conversationsCursor = await _conversations.FindAsync(cf).ConfigureAwait(false);
            var conversationsFound = await conversationsCursor.ToListAsync();

            var results = from uc in unreadConversations
                          join c in conversationsFound on uc.ConversationId equals c.Id
                          orderby uc.LastUpdated descending
                          select new ConversationModel
                          {
                              Id = c.Id,
                              Name = c.Name,
                              AuthorID = c.AuthorID,
                              UnreadMessages = uc.UnreadMessages
                          };

            return (total, results);
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

        public Task<ConversationModel> GetConversationByAuthorID(string authorId) => GetConversationByQueryAsync(c => c.AuthorID == authorId);        
        
        public Task<ConversationModel> GetConversationById(string conversationId) => GetConversationByQueryAsync(c => c.Id == conversationId);
        
        async Task<ConversationModel> GetConversationByQueryAsync(Expression<Func<Conversation, bool>> filter)
        {
            var existingConversation = (await _conversations.FindAsync(
                     new FilterDefinitionBuilder<Conversation>().Where(filter)).ConfigureAwait(false)
                    ).FirstOrDefault();

            if (existingConversation != null)
            {
                return new ConversationModel
                {
                    Id = existingConversation.Id,
                    Name = existingConversation.Name,
                    AuthorID = existingConversation.AuthorID
                };
            }

            return null;
        }
    }
}