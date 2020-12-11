using System;

namespace Avs.StaticSiteHosting.Web.Models.Identity
{
    public class User : BaseEntity
    {        
        public string Email { get; set; }
        public string Password { get; set; }
        public UserStatus Status { get; set; }               
        public Role[] Roles { get; set; }
        public DateTime DateJoined { get; set; }
        public DateTime? LastLocked { get; set; }
        public DateTime? LastLogin { get; set; }
        public int LocksAmount { get; set; }
        public string Comment { get; set; }
    }

    public enum UserStatus
    {
        Active,
        Locked
    }
}