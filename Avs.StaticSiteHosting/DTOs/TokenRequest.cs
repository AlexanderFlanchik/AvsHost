using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.DTOs
{
    public class TokenRequest
    {
        /// <summary>
        /// User login or email.
        /// </summary>
        [Required]
        public string Login { get; set; }
        
        /// <summary>
        /// User password.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}