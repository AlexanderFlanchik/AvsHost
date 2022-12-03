using System;
using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class LoginRequestModel
    {
        [Required]
        [MinLength(3)]
        public string Login { get; set; }
        
        [Required]
        [MinLength(4)]
        public string Password { get; set; }
    }

    public record UserInfo(string Name, string Email, bool isAdmin);
    public record LoginResponse(string token, DateTime expires_at, UserInfo userInfo);
}