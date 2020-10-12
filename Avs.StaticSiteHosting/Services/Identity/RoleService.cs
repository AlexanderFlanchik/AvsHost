using Avs.StaticSiteHosting.Web.Models.Identity;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Identity
{
    public class RoleService : IRoleService
    {
        private readonly IMongoCollection<Role> _roles;

        public RoleService(MongoEntityRepository entityRepository)
        {
            _roles = entityRepository.GetEntityCollection<Role>(GeneralConstants.ROLES_COLLECTION);
        }

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return (await _roles.FindAsync(r => r.Name == roleName).ConfigureAwait(false))
                    .FirstOrDefault();
        }
    }
}