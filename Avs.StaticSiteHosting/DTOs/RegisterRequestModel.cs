using System.ComponentModel.DataAnnotations;

namespace Avs.StaticSiteHosting.DTOs
{
    public class RegisterRequestModel : ProfileModel
    {           
        [Required]
        public string Password { get; set; }
    }

    public class ProfileModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }
    }

    public class ChangePasswordRequestModel
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string NewPassword { get; set; }
    }
}