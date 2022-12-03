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
        private readonly Func<string, IActionResult> badRequestResponse = (errorMessage) => new BadRequestObjectResult(new { error = errorMessage });
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

            var user = await _userService.GetUserByLoginAsync(login);
            
            if (user is null)
            {
                return badRequestResponse($"No user with login '{login}' has been found.");
            }

            if (!pwdHasher.VerifyPassword(user.Password, password))
            {
                return badRequestResponse("Invalid password entered.");
            }

            var (tokenValue, expiresAt) = jwtTokenProvider.GenerateToken(user);
            _logger.LogInformation($"Requested token for {login} at {DateTime.UtcNow} (UTC), succeded.");

            var userInfo = new UserInfo(user.Name, user.Email, _userService.IsAdmin(user));
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