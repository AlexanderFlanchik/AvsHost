using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models.Identity;
using Avs.StaticSiteHosting.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AuthController : Controller
    {
        private const string DEFAULT_USER_ROLE = "DefaultUser";
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Role> _roles;

        public AuthController(MongoEntityRepository entityRepository)
        {
            _users = entityRepository.GetEntityCollection<User>("Users");
            _roles = entityRepository.GetEntityCollection<Role>("Roles");
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> GetAccessToken(LoginRequestModel loginModel, [FromServices] PasswordHasher pwdHasher)
        {
            var login = loginModel.Login;
            var password = loginModel.Password;

            var user = (await _users.FindAsync(u => u.Email == login || u.Name == login)
                    .ConfigureAwait(false))
                    .FirstOrDefault();            

            if (user == null)
            {
                return BadRequest(new { error = $"No user with login '{login}' has been found." });
            }

            if (!pwdHasher.VerifyPassword(user.Password, password))
            {
                return BadRequest(new { error = "Invalid password entered." });
            }
           
            if (user.Status != UserStatus.Active)
            {
                return BadRequest(new { error = "Your account has been locked. Please contact administrator." });
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
            => Json(!(await _users.FindAsync(u => u.Email == email || u.Name == userName)
                   .ConfigureAwait(false))
                   .Any());
        
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterRequestModel registerRequest, [FromServices] PasswordHasher passwordHasher)
        {
            var alreadyExistedUser = (await _users.FindAsync(u => 
                    u.Name == registerRequest.UserName || 
                    u.Email == registerRequest.Email).ConfigureAwait(false)
                ).Any();

            if (alreadyExistedUser)
            {
                return Conflict("User already exists.");
            }

            var newUser = new User { Email = registerRequest.Email, Name = registerRequest.UserName };
            newUser.Password = passwordHasher.HashPassword(registerRequest.Password);
            newUser.Status = UserStatus.Active;

            var userRole = (await _roles.FindAsync(r => r.Name == DEFAULT_USER_ROLE).ConfigureAwait(false))
                    .FirstOrDefault();

            if (userRole == null)
            {
                userRole = new Role { Name = DEFAULT_USER_ROLE };
            }

            newUser.Roles = new[] { userRole };
            try
            {
                await _users.InsertOneAsync(newUser).ConfigureAwait(false);
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