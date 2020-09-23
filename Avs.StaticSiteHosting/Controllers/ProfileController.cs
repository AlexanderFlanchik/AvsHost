using System;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models.Identity;
using Avs.StaticSiteHosting.Services;
using Avs.StaticSiteHosting.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public ProfileController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }
        
        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateProfile(ProfileModel profileModel)
        {
            var (user, userValidationResult) = await ValidateCurrentUserAsync();
            if (userValidationResult != null)
            {
                return userValidationResult;
            }

            user.Email = profileModel.Email;
            user.Name = profileModel.UserName;

            await _userService.UpdateUserAsync(user).ConfigureAwait(false);

            return Ok();
        }

        [HttpPost]
        [Route("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestModel requestModel, [FromServices] PasswordHasher passwordHasher)
        {
            var newPassword = requestModel.NewPassword;
            var pwd = requestModel.Password;

            var (user, userValidationResult) = await ValidateCurrentUserAsync();
            if (userValidationResult != null)
            {
                return userValidationResult;
            }

            if (!passwordHasher.VerifyPassword(user.Password, pwd))
            {
                return BadRequest("Current password validation failed.");
            }

            user.Password = passwordHasher.HashPassword(newPassword);

            await _userService.UpdateUserAsync(user).ConfigureAwait(false);
            
            return Ok();
        }

        private async Task<(User, IActionResult)> ValidateCurrentUserAsync()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == AuthSettings.UserIdClaim)?.Value;
            IActionResult result = null;
            if (string.IsNullOrEmpty(userId))
            {
                result = BadRequest($"Invalid user ID: {userId}");
            }

            var user = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                result =  BadRequest($"No user with ID {userId} found.");
            }

            return (user, result);
        }
    }
}