using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.Identity;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IRoleService roleService, ILogger<AuthController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _logger = logger;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> GetAccessToken(LoginRequestModel loginModel, PasswordHasher pwdHasher, IJwtTokenProvider jwtTokenProvider)
        {
            var login = loginModel.Login;
            var password = loginModel.Password;

            _logger.LogInformation("Received an access token request from a user with login: {0}", login);

            var user = await _userService.GetUserByLoginAsync(login);
            
            if (user is null)
            {
                _logger.LogWarning($"Login failed - invalid login \"{login}\" has been entered.");
                
                return BadRequest(new { error = $"Error: no user with login \"{login}\" has been found." });   
            }

            var userName = user.Name;

            if (!pwdHasher.VerifyPassword(user.Password, password))
            {
                _logger.LogWarning($"Login failed - the user {userName} has entered invalid password.");
                
                return Unauthorized("Invalid password entered.");
            }

            // Generate a token for user verified
            var (tokenValue, expiresAt) = jwtTokenProvider.GenerateToken(user); 
            
            var lastLoginTimestamp = DateTime.UtcNow;
            
            _logger.LogInformation($"Requested authentication token for the user {userName} at {lastLoginTimestamp} (UTC), succeded.");

            // Update last login timestamp for the user
            user.LastLogin = lastLoginTimestamp;

            await _userService.UpdateUserAsync(user);

            var userInfo = new UserInfo(userName, user.Email, _userService.IsAdmin(user));
            var response = new LoginResponse(tokenValue, expiresAt, userInfo); 

            return Ok(response);
        }                

        [HttpGet]
        [Route("validateUserData")]
        public async Task<IActionResult> ValidateUserData(string userName, string email) 
            => Json(!await _userService.CheckUserExistsAsync(userName, email));
        
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterRequestModel registerRequest, [FromServices] PasswordHasher passwordHasher)
        {
            var userName = registerRequest.UserName;
            var email = registerRequest.Email;

            var alreadyExistedUser = await _userService.CheckUserExistsAsync(userName, email);
            if (alreadyExistedUser)
            {
                return Conflict("User already exists.");
            }

            var newUser = new User() 
                { 
                    Email = email, 
                    Name = userName,
                    Password = passwordHasher.HashPassword(registerRequest.Password),
                    Status = UserStatus.Active
                };
            
            var userRole = await _roleService.GetRoleByNameAsync(GeneralConstants.DEFAULT_USER_ROLE);            
            newUser.Roles = new[] { userRole };

            await _userService.CreateUserAsync(newUser);

            _logger.LogInformation($"Registered: {newUser.Name}.");

            return Ok();
        }
    }
}