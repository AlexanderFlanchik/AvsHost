namespace Avs.StaticSiteHosting.Models
{
    public abstract class RoleAccessEntity : BaseEntity   
    {
        public string[] RolesAllowed { get; set; }
    }
}