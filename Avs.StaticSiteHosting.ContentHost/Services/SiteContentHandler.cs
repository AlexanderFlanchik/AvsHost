using System.Net;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Contracts;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Configuration;
using Avs.StaticSiteHosting.Shared.Contracts;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Avs.StaticSiteHosting.ContentHost.Services;

public interface ISiteContentHandler
{
    /// <summary>
    /// Handles site content using site info instance
    /// </summary>
    /// <param name="siteInfo">Site info</param>
    /// <param name="siteName">Site Name (request parameter)</param>
    /// <param name="sitePath">Site Path (request parameter)</param>
    /// <returns></returns>
    ValueTask<HandleContentResult> ValidateAndProcessContent(SiteContentInfo? siteInfo, string siteName,
        string sitePath);
}

public class SiteContentHandler(
    CloudStorageSettings cloudStorageSettings, 
    ICloudStorageProvider cloudStorageProvider,
    CloudStorageWorker storageWorker,
    IOptions<StaticSiteOptions> staticSiteOptions): ISiteContentHandler
{
    public async ValueTask<HandleContentResult> ValidateAndProcessContent(SiteContentInfo? siteInfo, string siteName, string sitePath)
    {
       if (siteInfo is null || !siteInfo.IsActive)
       {
           return new HandleContentResult((int)HttpStatusCode.NotFound, "The resource cannot be found or its inactive. Please check URL.");
       }
       
       string siteOwnerId = siteInfo.User.Id;
       if (!siteInfo.User.IsActive)
       {
           return HandleContentResult.ContentBlocked("Cannot provide content. Site owner is blocked.", siteInfo.Id, siteOwnerId);
       }

       var (contentItem, fileName) = siteInfo.GetContentItem(sitePath);
       if (contentItem is null)
       {
           return HandleContentResult.NotFound($"No content with path '{sitePath}' found.", siteInfo.Id, siteOwnerId);
       }

       var siteContentPath = Path.Combine(staticSiteOptions.Value.ContentPath, siteName);
       var fileProvider = new PhysicalFileProvider(new DirectoryInfo(siteContentPath).FullName);
       IFileInfo fileInfo = fileProvider.GetFileInfo(fileName);
       if (fileInfo.Exists)
       {
           return HandleContentResult.Success(siteInfo.Id, fileInfo, contentItem.ContentType, contentItem.CacheDuration);
       }

       if (!cloudStorageSettings.Enabled)
       {
           return HandleContentResult.NotFound($"No physical file found for path '{sitePath}'.", siteInfo.Id, siteOwnerId);
       }

       await using var cloudStream = await cloudStorageProvider.GetCloudContent(siteInfo.User.UserName, siteInfo.Name, fileInfo.Name);
       if (cloudStream is null)
       {
           return HandleContentResult.NotFound($"No content file found: '{fileInfo.Name}'.", siteInfo.Id, siteOwnerId);
       }

       storageWorker.Add(new SyncContentTask(fileInfo.Name, fileInfo.PhysicalPath!, siteInfo.User.UserName, siteInfo.Name));

       return HandleContentResult.Success(siteInfo.Id, cloudStream, contentItem.ContentType, contentItem.CacheDuration);
    }
}