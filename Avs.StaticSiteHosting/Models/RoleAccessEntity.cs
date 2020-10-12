namespace Avs.StaticSiteHosting.Web.Models
{
    public abstract class RoleAccessEntity : BaseEntity   
    {
        public string[] RolesAllowed { get; set; }
    }
}