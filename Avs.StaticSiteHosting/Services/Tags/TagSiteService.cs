using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Avs.StaticSiteHosting.Web.Models;

namespace Avs.StaticSiteHosting.Web.Services
{
    public interface ITagSiteService
    {
        /// <summary>
        /// Checks if a tag with ID specified is used in sites.
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <returns>true if the tag is used, false othewise.</returns>
        Task<bool> IsTagUsedInSites(string tagId);
        
        /// <summary>
        /// Removes tag references in sites using the tag ID specifed.
        /// </summary>
        /// <param name="tagId">Tag ID</param>
        /// <returns></returns>
        Task RemoveTagFromSites(string tagId);
    }

    public class TagSiteService : ITagSiteService
    {
        private readonly IMongoCollection<Site> _sites;
     
        public TagSiteService(MongoEntityRepository entityRepository)
        {
            _sites = entityRepository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);
        }

        public async Task RemoveTagFromSites(string tagId)
        {
            var sitesQuery = await BuildSitesQuery(tagId);
            var sites = await sitesQuery.ToListAsync();

            foreach (var site in sites)
            {
                var newTags = site.TagIds.Where(t => t.Id != tagId).ToArray();
                await _sites.UpdateOneAsync(new FilterDefinitionBuilder<Site>().Where(s => s.Id == site.Id),
                    new UpdateDefinitionBuilder<Site>().Set(s => s.TagIds, newTags.Any() ? newTags : null)
                );
            }
        }

        public async Task<bool> IsTagUsedInSites(string tagId)
        {
            var sitesQuery = await BuildSitesQuery(tagId);
            return await sitesQuery.AnyAsync();
        }

        private Task<IAsyncCursor<Site>> BuildSitesQuery(string tagId)
            => _sites.FindAsync(
                new FilterDefinitionBuilder<Site>()
                    .Where(s => 
                        s.TagIds != null && 
                        s.TagIds.Any(tId => tId.Id == tagId))
                    );
    }
}