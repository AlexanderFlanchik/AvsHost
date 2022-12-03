using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Avs.StaticSiteHosting.Web.Models.Identity;

namespace Avs.StaticSiteHosting.Web.Services.Identity;

public interface IJwtTokenProvider
{
    (string token, DateTime expiresAt) GenerateToken(User user);
}

public class JwtTokenProvider : IJwtTokenProvider
{
    public (string, DateTime) GenerateToken(User user)
    {
        var currentTimestamp = DateTime.UtcNow;
        var tokenLifeTime = AuthSettings.TokenLifetime;
        var signingCredentials = new SigningCredentials(AuthSettings.SecurityKey(), SecurityAlgorithms.HmacSha256);
        var expiresAt = currentTimestamp.Add(tokenLifeTime);

        var claims = new List<Claim>
            {
                new Claim(AuthSettings.UserIdClaim, user.Id),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name)
            };

        var jwtToken = new JwtSecurityToken(issuer: AuthSettings.ValidIssuer,
                audience: AuthSettings.ValidAudience,
                claims: claims.Union(user.Roles.Select(r => new Claim(ClaimsIdentity.DefaultRoleClaimType, r.Name)).ToArray()),
                notBefore: currentTimestamp,
                expires: expiresAt,
                signingCredentials: signingCredentials
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(jwtToken),
            expiresAt
        );
    }
}