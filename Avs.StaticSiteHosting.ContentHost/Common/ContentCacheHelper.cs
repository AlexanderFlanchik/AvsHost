namespace Avs.StaticSiteHosting.ContentHost.Common
{
    internal static class ContentCacheHelper
    {
        public static string GetContentCacheKey(string siteName, string sitePath) => $"{siteName}_{sitePath}";
    }
}
