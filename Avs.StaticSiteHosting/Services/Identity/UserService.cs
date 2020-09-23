using Avs.StaticSiteHosting.Models.Identity;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(MongoEntityRepository entityRepository)
        {
            _users = entityRepository.GetEntityCollection<User>(GeneralConstants.USERS_COLLECTION);
        }

        public async Task<bool> CheckUserExistsAsync(string userName, string email)
        {
            return (await _users.FindAsync(u => u.Email == email || u.Name == userName).ConfigureAwait(false))
                   .Any();            
        }

        public async Task CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user).ConfigureAwait(false);
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return (await _users.FindAsync(u => u.Id == userId).ConfigureAwait(false)).FirstOrDefault();
        }

        public async Task<User> GetUserByLoginAsync(string login)
        {
            return (await _users.FindAsync(u => u.Email == login || u.Name == login).ConfigureAwait(false))
                .FirstOrDefault();
        }

        public bool IsAdmin(User user)
        {
            return user.Roles.Any(r => r.Name == GeneralConstants.ADMIN_ROLE);
        }

        public async Task UpdateUserAsync(User user)
        {
            var filter = new FilterDefinitionBuilder<User>().Where(u => u.Id == user.Id);
            var update = new UpdateDefinitionBuilder<User>()
                .Set(u => u.Name, user.Name)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Password, user.Password);

            await _users.UpdateOneAsync(filter, update).ConfigureAwait(false);
        }
    }
}