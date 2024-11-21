namespace Avs.StaticSiteHosting.Shared.Contracts
{
    public class UserDetails
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; } = default!;

        /// <summary>
        /// Shows if a user is active (or banned)
        /// </summary>
        public bool IsActive { get; set; }
    }
}