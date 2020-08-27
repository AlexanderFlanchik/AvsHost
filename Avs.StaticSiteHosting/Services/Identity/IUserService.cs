using Avs.StaticSiteHosting.Models.Identity;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services.Identity
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(string userId);
        Task<User> GetUserByLoginAsync(string login);
        Task<bool> CheckUserExistsAsync(string userName, string email);
        Task CreateUserAsync(User user);
        bool IsAdmin(User user);
    }
}