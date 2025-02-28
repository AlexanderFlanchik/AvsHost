using Avs.StaticSiteHosting.Shared.Contracts;

namespace Avs.StaticSiteHosting.ContentHost.Common
{
    public static class SiteContentInfoExtensions
    {
        public static (ContentItemInfo?, string) GetContentItem(this SiteContentInfo contentInfo, string sitePath)
        {
            ArgumentNullException.ThrowIfNull(contentInfo);

            if (!contentInfo.ContentItems.Any())
            {
                return (null, string.Empty);
            }

            string? mappedSitePath = null;
            contentInfo.Mappings?.TryGetValue(sitePath, out mappedSitePath);
           
            var fileName = !string.IsNullOrEmpty(mappedSitePath) ? mappedSitePath : sitePath;
            var contentItem = contentInfo.ContentItems.FirstOrDefault(ci =>
                ci.FileName == fileName || $"{ci.DestinationPath.Replace('\\', '/')}/{ci.FileName}" == fileName
             );

            return (contentItem, fileName);
        }

        public static string[] GetSitePaths(this SiteContentInfo contentInfo)
        {
            ArgumentNullException.ThrowIfNull(contentInfo);
            
            var paths = new List<string>();
            
            foreach (var item in contentInfo.ContentItems)
            {
                if (string.IsNullOrEmpty(item.DestinationPath))
                {
                    paths.Add(item.FileName);
                }
                else
                {
                    var mappingKvp = contentInfo.Mappings?.FirstOrDefault(m => m.Value == item.DestinationPath);
                    if (mappingKvp is not null)
                    {
                        paths.Add(mappingKvp.Value.Key);
                    }
                    else
                    {
                        paths.Add($"{item.DestinationPath.Replace('\\', '/')}/{item.FileName}");
                    }
                }
            }

            return [.. paths];
        }

        public static bool IsSiteVisited(this SiteContentInfo siteInfo, string sitePath)
        {
            return !string.IsNullOrEmpty(siteInfo.LandingPage) && siteInfo.LandingPage == sitePath;
        }
    }
}