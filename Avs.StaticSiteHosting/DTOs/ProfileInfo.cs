using Avs.StaticSiteHosting.Web.Models.Identity;

namespace Avs.StaticSiteHosting.Web.DTOs
{
    public class ProfileInfo
    {
        public string UserId { get; set; }
        public UserStatus Status { get; set; }
        public string Comment { get; set; }
        public int? UnreadMessages { get; set; }
    }
}