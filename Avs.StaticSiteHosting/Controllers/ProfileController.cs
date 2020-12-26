using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Hubs;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Avs.StaticSiteHosting.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserService userService, ILogger<ProfileController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger;
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

        [HttpGet]
        [Route("user/{userId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserProfile([Required] string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);
                        
            return user == null ? (IActionResult)NotFound() :
             Ok(new UserProfileModel() 
                { 
                    Id = userId,
                    Name = user.Name,
                    Email = user.Email,
                    Status = user.Status,
                    DateJoined = user.DateJoined,
                    LastLocked = user.LastLocked,
                    Comment = user.Comment
                });            
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        [Route("UpdateUserProfile/{userId}")]
        public async Task<IActionResult> UpdateUserProfile(string userId, UserStatusModel updateRequest, [FromServices]IHubContext<UserNotificationHub> notificationHub, [FromServices]ISiteService siteService)
        {
            var user = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                return NotFound();
            }

            var newStatus = updateRequest.Status;
            var newComment = updateRequest.Comment;
            bool isModified = false;

            if (user.Status != newStatus)
            {
                if (newStatus == UserStatus.Locked)
                {
                    user.LastLocked = DateTime.Now;
                    user.LocksAmount++;
                    _logger.LogInformation($"User with ID = {userId} is banned by administrator at {user.LastLocked}.");
                }
                else
                {
                    _logger.LogInformation($"Ban is removed from the user ID = {userId} at {DateTime.UtcNow}.");
                }
                user.Status = newStatus;
                await siteService.UpdateSitesStatusAsync(userId, newStatus).ConfigureAwait(false);
                await notificationHub.Clients.User(userId).SendAsync("UserStatusChanged", new { currentStatus = newStatus });

                isModified = true;
            }

            if (user.Comment != newComment)
            {
                user.Comment = updateRequest.Comment;
                isModified = true;
            }
            
            if (!isModified)
            {
                return NoContent();
            }

            await _userService.UpdateUserAsync(user).ConfigureAwait(false);

            return Ok();
        }

        [HttpGet]
        [Route("profile-info")]
        public async Task<IActionResult> GetProfileInfo()
        {
            var userId = User.FindFirst(AuthSettings.UserIdClaim)?.Value;
            var profile = await _userService.GetUserByIdAsync(userId).ConfigureAwait(false);
            if (profile == null)
            {
                return NotFound();
            }    

            return Ok(new { profile.Status, profile.Comment });
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