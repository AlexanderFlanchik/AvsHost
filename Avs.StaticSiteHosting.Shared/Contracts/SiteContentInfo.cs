namespace Avs.StaticSiteHosting.Shared.Contracts
{
    public class SiteContentInfo
    {
        /// <summary>
        /// Site Id
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// Site name
        /// </summary>
        public string Name { get; set; } = default!;

        /// <summary>
        /// Site description
        /// </summary>
        public string Description { get; set; } = default!;

        /// <summary>
        /// Checks if the site is active and running
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Date of the site launch
        /// </summary>
        public DateTime? LaunchedOn { get; set; }
        
        /// <summary>
        /// Date of the latest site stopping
        /// </summary>
        public DateTime? LastStopped { get; set; }
        
        /// <summary>
        /// Resource mappings
        /// </summary>
        public IDictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Represents a landing page for the site
        /// </summary>
        public string? LandingPage { get; set; }

        /// <summary>
        /// Information about site owner
        /// </summary>
        public UserDetails User { get; set; } = default!;

        /// <summary>
        /// Site content
        /// </summary>
        public ContentItemInfo[] ContentItems { get; set; } = Array.Empty<ContentItemInfo>();
    }
}