using Avs.StaticSiteHosting.Web.Models.Identity;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Identity
{
    public interface IUserService
    {
        /// <summary>
        /// Gets a user by user ID.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns></returns>
        Task<User> GetUserByIdAsync(string userId);
        
        /// <summary>
        /// Gets a user by login.
        /// </summary>
        /// <param name="login">User login</param>
        /// <returns></returns>
        Task<User> GetUserByLoginAsync(string login);
        
        /// <summary>
        /// Checks if there is a user with the name or email specified.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="email">User email</param>
        /// <returns></returns>
        Task<bool> CheckUserExistsAsync(string userName, string email);
        
        /// <summary>
        /// Updates current user data.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task UpdateUserAsync(User user);
        
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task CreateUserAsync(User user);
        
        /// <summary>
        /// Checks if the user has Admin role.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool IsAdmin(User user);
    }
}