namespace Avs.StaticSiteHosting.Shared.Contracts
{
    public class GetSiteContentRequestMessage
    {
        /// <summary>
        /// Site name
        /// </summary>
        public string SiteName { get; set; } = default!;
    }
}