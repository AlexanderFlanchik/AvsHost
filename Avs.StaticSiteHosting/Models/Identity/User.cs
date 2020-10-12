using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Avs.StaticSiteHosting.Web.Models.Identity
{
    public class User : BaseEntity
    {        
        public string Email { get; set; }
        public string Password { get; set; }
        public UserStatus Status { get; set; }               
        public Role[] Roles { get; set; }
    }

    public enum UserStatus
    {
        Active,
        Locked
    }
}