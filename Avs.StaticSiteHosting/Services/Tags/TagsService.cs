using Avs.StaticSiteHosting.Web.DTOs;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tag = Avs.StaticSiteHosting.Web.Models.Tags.Tag;

namespace Avs.StaticSiteHosting.Web.Services
{
    public interface ITagsService
    {
        /// <summary>
        /// Gets all tags available for user specified.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of tags</returns>
        Task<IEnumerable<TagModel>> GetAllTags(string userId);
        
        /// <summary>
        /// Gets a tag by ID for user specified.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="tagId">Tag ID</param>
        /// <returns>Tag data if found null otherwise</returns>
        Task<TagModel> GetTagById(string userId, string tagId);
        
        /// <summary>
        /// Creates a tag using tag data and user ID.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newTagModel">Tag data (name, colors)</param>
        /// <returns>Tag created</returns>
        Task<TagModel> CreateTag(string userId, TagModel newTagModel);
        
        /// <summary>
        /// Deletes tag by ID
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <returns></returns>
        Task DeleteTag(string tagId);

        /// <summary>
        /// Checks if a tag with name specified already exists for a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="tagName">Tag name</param>
        /// <returns>true if the tag exists, false otherwise.</returns>
        Task<bool> TagExists(string userId, string tagName);
    }

    public class TagsService : ITagsService
    {
        private readonly IMongoCollection<Tag> _tags;
        private readonly ITagSiteService _tagSiteService;

        public TagsService(MongoEntityRepository repository, ITagSiteService tagSiteService)
        {
            _tags = repository.GetEntityCollection<Tag>(GeneralConstants.TAGS_COLLECTION);
            _tagSiteService = tagSiteService;
        }

        public async Task<IEnumerable<TagModel>> GetAllTags(string userId)
        {
            var tags = await _tags.FindAsync(new FilterDefinitionBuilder<Tag>().Where(x => x.UserId == userId));
            return (await tags.ToListAsync())
                    .Select(t => new TagModel(t.Id, t.Name, t.BackgroundColor, t.TextColor))
                    .ToArray();
        }

        public async Task<TagModel> CreateTag(string userId, TagModel newTagModel)
        {
            var newTag = new Tag()
            {
                UserId = userId,
                Name = newTagModel.Name,
                BackgroundColor = newTagModel.BackgroundColor,
                TextColor = newTagModel.TextColor
            };

            await _tags.InsertOneAsync(newTag);
            
            return new TagModel(newTag.Id, newTag.Name, newTag.BackgroundColor, newTag.TextColor);
        }

        public async Task DeleteTag(string tagId)
        {
            await _tagSiteService.RemoveTagFromSites(tagId);
            await _tags.DeleteOneAsync(new FilterDefinitionBuilder<Tag>().Where(t => t.Id == tagId));
        }

        public async Task<TagModel> GetTagById(string userId, string tagId)
        {
            var tagQuery = await _tags.FindAsync(new FilterDefinitionBuilder<Tag>().Where(x => x.Id == tagId && x.UserId == userId));
            var tag = await tagQuery.FirstOrDefaultAsync();
            if (tag is null)
            {
                return null;
            }

            return new TagModel(tagId, tag.Name, tag.BackgroundColor, tag.TextColor);
        }

        public async Task<bool> TagExists(string userId, string tagName)
        {
            var tagQuery = await _tags.FindAsync(new FilterDefinitionBuilder<Tag>().Where(x => x.Name == tagName && x.UserId == userId));
            return await tagQuery.AnyAsync();
        }
    }
}