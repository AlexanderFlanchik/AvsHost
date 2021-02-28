using Avs.StaticSiteHosting.Web.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.Identity
{
    public interface IUserRoleService
    {
        Task<IEnumerable<string>> GetAdminUserIds();
    }

    public class UserRoleService : IUserRoleService
    {
        private readonly IMongoCollection<User> _users;

        public UserRoleService(MongoEntityRepository entityRepository)
        {
            _users = entityRepository.GetEntityCollection<User>(GeneralConstants.USERS_COLLECTION);
        }

        public async Task<IEnumerable<string>> GetAdminUserIds()
        {
            var userQuery = await _users.FindAsync(new FilterDefinitionBuilder<User>()
                    .Where(u => u.Roles.Any(r => r.Name == GeneralConstants.ADMIN_ROLE))).ConfigureAwait(false);

            return (await userQuery.ToListAsync()).Select(u => u.Id);
        }
    }
}
