using Avs.StaticSiteHosting.ContentHost.Common;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;

namespace Avs.StaticSiteHosting.ContentHost.Services
{
    public class ContentCacheService(IMemoryCache memoryCache, ILogger<ContentCacheService> logger) : BackgroundService
    {
        private readonly Channel<ContentCache> _channel = Channel.CreateBounded<ContentCache>(
            new BoundedChannelOptions(10_000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });

        /// <summary>
        /// Adds a content (through stream) for on-demand cache processing.
        /// The stream will be disposed in worker.
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="contentType">Content type</param>
        /// <param name="contentStream">Content stream</param>
        public void AddContentToCache(string cacheKey, string contentType, Stream contentStream, TimeSpan cacheDuration)
        {
           _channel.Writer.TryWrite(new ContentCache(cacheKey, contentType, contentStream, cacheDuration));
        }

        /// <summary>
        /// Returns site content from memory cache
        /// </summary>
        /// <param name="cacheKey">Cache key</param>
        /// <returns>A task which returns a content if it exists in cache or null otherwise.</returns>
        public ValueTask<ContentCacheEntry?> GetContentCacheAsync(string cacheKey)
        {
            if (memoryCache.TryGetValue(cacheKey, out ContentCacheEntry? entry))
            {
                return ValueTask.FromResult(entry);
            }

            return ValueTask.FromResult<ContentCacheEntry?>(null);
        }

        public void RemoveCacheEntry(string cacheKey)
        {
           memoryCache.Remove(cacheKey);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var contentCache = await _channel.Reader.ReadAsync(stoppingToken);
                    using (contentCache.ContentStream)
                    {
                        using var ms = new MemoryStream();
                        await contentCache.ContentStream.CopyToAsync(ms);
                        var content = ms.ToArray();

                        var cacheEntry = new ContentCacheEntry(content, contentCache.ContentType);
                        memoryCache.Set(contentCache.CacheKey, cacheEntry, contentCache.CacheDuration);
                    }
                }
                catch (Exception ex) 
                {
                    logger.LogError(ex, "Error during processing content cache.");
                }
            }
        }

        internal record ContentCache(string CacheKey, string ContentType, Stream ContentStream, TimeSpan CacheDuration);
    }
}