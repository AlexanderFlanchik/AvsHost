using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models.Identity;
using Avs.StaticSiteHosting.Services;
using Avs.StaticSiteHosting.Services.Identity;

namespace Avs.StaticSiteHosting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AuthController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        
        public AuthController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> GetAccessToken(LoginRequestModel loginModel, [FromServices] PasswordHasher pwdHasher)
        {
            var login = loginModel.Login;
            var password = loginModel.Password;

            var user = await _userService.GetUserByLoginAsync(login);
            Func<string, IActionResult> badRequestResponse = (errorMessage) => BadRequest(new { error = errorMessage });

            if (user == null)
            {
                return badRequestResponse($"No user with login '{login}' has been found.");
            }

            if (!pwdHasher.VerifyPassword(user.Password, password))
            {
                return badRequestResponse("Invalid password entered.");
            }
           
            if (user.Status != UserStatus.Active)
            {
                return badRequestResponse("Your account has been locked. Please contact administrator.");
            }

            var currentTimestamp = DateTime.UtcNow;
            var tokenLifeTime = AuthSettings.TokenLifetime;
            var signingCredentials = new SigningCredentials(AuthSettings.SecurityKey(), SecurityAlgorithms.HmacSha256);
            var expiresAt = currentTimestamp.Add(tokenLifeTime);

            var jwtToken = new JwtSecurityToken(issuer: AuthSettings.ValidIssuer, 
                    audience: AuthSettings.ValidAudience, 
                    claims: user.Roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r.Name)).ToArray(),
                    notBefore: currentTimestamp, 
                    expires: expiresAt, 
                    signingCredentials: signingCredentials
            );

            var encodedToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);
            System.Diagnostics.Debug.WriteLine($"Requested token for {login} at {DateTime.UtcNow} (UTC), succeded.");
            
            return Ok(new { 
                token = encodedToken,
                expires_at = expiresAt,
                userInfo = new { user.Name, user.Email }
            });
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

            var newUser = new User { Email = email, Name = userName };
            newUser.Password = passwordHasher.HashPassword(registerRequest.Password);
            newUser.Status = UserStatus.Active;

            try
            {
                var userRole = await _roleService.GetRoleByNameAsync(GeneralConstants.DEFAULT_USER_ROLE);
                if (userRole == null)
                {
                    throw new InvalidOperationException($"'{GeneralConstants.DEFAULT_USER_ROLE}' not found.");
                }

                newUser.Roles = new[] { userRole };

                await _userService.CreateUserAsync(newUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Problem($"Cannot create a new user.{ex.Message}.");
            }

            return Ok();
        }
    }
}