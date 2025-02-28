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
        /// <returns></returns>
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
        /// <returns></returns>
        Task SetSiteConfig(SiteContentInfo? siteContent);

        /// <summary>
        /// Returns a list of sites served from the content server instance
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetSitesList();
    }

    public class SiteContentProvider : ISiteContentProvider
    {
        private readonly ConcurrentDictionary<string, SiteContentInfo> _sites = new ConcurrentDictionary<string, SiteContentInfo>();
        private readonly ISiteContentRequester _contentRequester;

        public SiteContentProvider(ISiteContentRequester contentRequester)
        {
           _contentRequester = contentRequester;
        }

        public async Task<SiteContentInfo?> GetSiteContentByName(string siteName)
        {
            if (_sites.TryGetValue(siteName, out var si))
            {
                return si;
            }

            SiteContentInfo? siteContentInfo = await _contentRequester.RequestSiteContentAsync(siteName);
            if (siteContentInfo is null)
            {
                return null;
            }

            _sites[siteName] = siteContentInfo;

            return siteContentInfo;
        }

        public SiteContentInfo? GetSiteContentById(string siteId)
            => _sites.Values.FirstOrDefault(s => s.Id == siteId);

        public Task SetSiteConfig(SiteContentInfo? siteContent)
        {
            if (siteContent is not null)
            {
                _sites[siteContent.Name] = siteContent;
            }

            return Task.CompletedTask;
        }

        public IEnumerable<string> GetSitesList() => _sites.Keys;
    }
}