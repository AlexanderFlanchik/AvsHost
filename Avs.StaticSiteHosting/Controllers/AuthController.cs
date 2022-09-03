using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.Identity;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

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
        public async Task<IActionResult> GetAccessToken(LoginRequestModel loginModel, [FromServices] PasswordHasher pwdHasher)
        {
            var login = loginModel.Login;
            var password = loginModel.Password;
            var user = await _userService.GetUserByLoginAsync(login);
            
            if (user == null)
            {
                return badRequestResponse($"No user with login '{login}' has been found.");
            }

            if (!pwdHasher.VerifyPassword(user.Password, password))
            {
                return badRequestResponse("Invalid password entered.");
            }
                     
            var currentTimestamp = DateTime.UtcNow;
            var tokenLifeTime = AuthSettings.TokenLifetime;
            var signingCredentials = new SigningCredentials(AuthSettings.SecurityKey(), SecurityAlgorithms.HmacSha256);
            var expiresAt = currentTimestamp.Add(tokenLifeTime);

            var claims = new List<Claim>();
            claims.Add(new Claim(AuthSettings.UserIdClaim, user.Id));
            claims.Add(new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name));
            
            var jwtToken = new JwtSecurityToken(issuer: AuthSettings.ValidIssuer, 
                    audience: AuthSettings.ValidAudience, 
                    claims: claims.Union(user.Roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r.Name)).ToArray()),
                    notBefore: currentTimestamp, 
                    expires: expiresAt, 
                    signingCredentials: signingCredentials
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            _logger.LogInformation($"Requested token for {login} at {DateTime.UtcNow} (UTC), succeded.");

            var response = new
            {
                token = encodedToken,
                expires_at = expiresAt,
                userInfo = new
                {
                    user.Name,
                    user.Email,
                    isAdmin = _userService.IsAdmin(user)
                }
            };

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