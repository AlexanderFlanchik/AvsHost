using Avs.StaticSiteHosting.Web.Models.Identity;
using System;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class UserProfileModel : UserStatusModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime? LastLocked { get; set; }
    }

    public class UserStatusModel
    {
        public UserStatus Status { get; set; }
        public string Comment { get; set; }
    }
}