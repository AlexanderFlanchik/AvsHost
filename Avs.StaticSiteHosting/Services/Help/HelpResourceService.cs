using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services
{
    public class HelpResourceService : IHelpResourceService
    {
        private readonly IMongoCollection<HelpResource> _helpResources;
        public HelpResourceService(MongoEntityRepository entityRepository)
        {
            _helpResources = entityRepository.GetEntityCollection<HelpResource>(GeneralConstants.HELPRESOURCE_COLLECTION);
        }

        public async Task<HelpResource> GetHelpResourceAsync(string name)
        {
            var resourceCursor = await _helpResources.FindAsync(r => r.Name == name).ConfigureAwait(false);
            
            return await resourceCursor.FirstOrDefaultAsync().ConfigureAwait(false);            
        }
    }
}