using Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;
using Avs.StaticSiteHosting.Shared.Contracts;
using System.Collections.Concurrent;

namespace Avs.StaticSiteHosting.ContentHost.Services
{
    public interface ISiteContentProvider
    {
        /// <summary>
        /// Returns information about site using site name
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>An async operation which returns a <see cref="SiteContentInfo"/> when resolved</returns>
        Task<SiteContentInfo?> GetSiteContentByName(string siteName);

        /// <summary>
        /// Returns a site content instance by Id looking into local set of sites
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <returns>Site info if locally there is a site with ID specified, or null otherwise.</returns>
        SiteContentInfo? GetSiteContentById(string siteId);

        /// <summary>
        /// Adds or updates site content info in the memory or other configuration storage
        /// </summary>
        /// <param name="siteContent"></param>
        /// <returns>A task instance that represents an async operation</returns>
        Task SetSiteConfig(SiteContentInfo? siteContent);

        /// <summary>
        /// Returns a list of sites served from the content server instance
        /// </summary>
        IEnumerable<string> GetSitesList();
        
        /// <summary>
        ///  Clears site info for outdated site cache entries
        /// <param name="expiration">Duration of site info cache</param>
        /// </summary>
        void ClearSiteCache(TimeSpan expiration);
    }

    public class SiteContentProvider(ISiteContentRequester contentRequester) : ISiteContentProvider
    {
        private readonly ConcurrentDictionary<string, SiteContentInfoEntry> _sites = new();

        public async Task<SiteContentInfo?> GetSiteContentByName(string siteName)
        {
            if (_sites.TryGetValue(siteName, out var si))
            {
                si.Timestamp = DateTime.UtcNow;
                return si.SiteContentInfo;
            }

            SiteContentInfo? siteContentInfo = await contentRequester.RequestSiteContentAsync(siteName);
            if (siteContentInfo is null)
            {
                return null;
            }

            _sites[siteName] = new SiteContentInfoEntry(DateTime.UtcNow, siteContentInfo);

            return siteContentInfo;
        }

        public SiteContentInfo? GetSiteContentById(string siteId)
            => _sites.Values.FirstOrDefault(s => s.SiteContentInfo.Id == siteId)?.SiteContentInfo;

        public Task SetSiteConfig(SiteContentInfo? siteContent)
        {
            if (siteContent is not null)
            {
                _sites[siteContent.Name] = new SiteContentInfoEntry(DateTime.UtcNow, siteContent);
            }

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetSitesList() => _sites.Keys;

        public void ClearSiteCache(TimeSpan expiration)
        {
            var keysToDelete = _sites
                .Where(s => s.Value.Timestamp + expiration < DateTime.UtcNow)
                .Select(s => s.Key)
                .ToArray();
            
            foreach (var key in keysToDelete)
            {
                _sites.TryRemove(key, out _);
            }
        }
        
        private class SiteContentInfoEntry(DateTime dateAdd, SiteContentInfo siteContentInfo)
        {
            public DateTime Timestamp { get; set; } = dateAdd;
            public SiteContentInfo SiteContentInfo { get; } = siteContentInfo;
        }
    }
}