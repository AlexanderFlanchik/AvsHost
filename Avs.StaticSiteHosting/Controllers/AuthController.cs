using Avs.StaticSiteHosting.Models.Identity;
using Avs.StaticSiteHosting.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;

        public AuthController(MongoEntityRepository entityRepository)
        {
            _users = entityRepository.GetEntityCollection<User>("Users");
        }

        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> Token([Required]string login, [Required]string password, [FromServices] PasswordHasher pwdHasher)
        {
            var user = (await _users.FindAsync(u => u.Email == login || u.Name == login).ConfigureAwait(false))
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
                expires_at = expiresAt
            });
        }                
    }
}
