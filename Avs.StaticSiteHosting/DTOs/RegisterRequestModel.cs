using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.DTOs
{
    public class RegisterRequestModel
    {        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public string UserName { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
