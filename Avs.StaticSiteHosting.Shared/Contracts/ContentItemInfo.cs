namespace Avs.StaticSiteHosting.Shared.Contracts
{
    public class ContentItemInfo
    {
        /// <summary>
        /// Content file ID
        /// </summary>
        public string Id { get; set; } = default!;
        
        /// <summary>
        /// File name
        /// </summary>
        public string FileName { get; set; } = default!;
       
        /// <summary>
        /// Destination path on disk or network
        /// </summary>
        public string DestinationPath { get; set; } = default!;
        
        /// <summary>
        /// Content type
        /// </summary>
        public string ContentType { get; set; } = string.Empty;
    }
}