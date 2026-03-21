using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Avs.StaticSiteHosting.Shared.Configuration;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement;

public interface ISitemapManager
{
    /// <summary>
    /// Creates a sitemap.xml file for site and stores it.
    /// </summary>
    /// <param name="siteId">Site ID, if a site is not new</param>
    /// <returns>A task which returns a sitemap size in Kb when completes</returns>
    Task<SitemapResult> CreateOrUpdateSitemapAsync(string siteId);
}

public class SitemapManager(
    ISiteService siteService,
    IContentManager contentManager,
    MongoEntityRepository repository,
    ILogger<SitemapManager> logger,
    IConfiguration config,
    IOptions<StaticSiteOptions> staticSiteOptions) : ISitemapManager
{
    private const string HTML_EXTENSION = ".html";
    private const string HTML_CONTENT_TYPE = "text/html";
    private const string XML_CONTENT_TYPE = "text/xml";
    private const string SITEMAP_FILE_NAME = "sitemap.xml";

    private readonly IMongoCollection<ContentItem> _contentItemsCollection =
        repository.GetEntityCollection<ContentItem>(GeneralConstants.CONTENT_ITEMS_COLLECTION);
    
    private readonly string _contentHostUrl = Environment.GetEnvironmentVariable("CONTENT_HOST_URL")
                                              ?? config["ContentHostUrl"];
    
    public async Task<SitemapResult> CreateOrUpdateSitemapAsync(string siteId)
    {
        var site = await siteService.GetSiteByIdAsync(siteId);
        if (site is null)
        {
            logger.LogWarning("Site {siteId} not found", siteId);
            return SitemapResult.Fail("Site not found");
        }

        var contentItems = (await contentManager.GetUploadedContentAsync(site.Id)).ToArray();
        var pageItems = contentItems
            .Where(i => 
                i.FileName.EndsWith(HTML_EXTENSION) || i.ContentType == HTML_CONTENT_TYPE
        ).ToList();

        var siteFolder = Path.Combine(staticSiteOptions.Value.ContentPath, site.Name);
        if (!Directory.Exists(siteFolder))
        {
            logger.LogWarning("Site folder {siteFolder} not found", siteFolder);

            return SitemapResult.Fail("Site folder not found");
        }
        
        var data = new UrlSet()
            {
                Urls = pageItems.Select(i => 
                    new Url()
                    {
                        Location = GetPageUrl(i, site),
                        Modified = (i.UpdateDate ?? i.UploadedAt).ToString("yyyy-MM-dd")
                    }).ToArray()
            };
        
        try
        {
            var sitemapPath = Path.Combine(siteFolder, SITEMAP_FILE_NAME);
            var siteMapSize = Math.Round((decimal)await SerializeAndSaveSitemapAsync(sitemapPath, data) / 1024, 2);
            var mapId = await CreateOrUpdateContentItemAsync(site, contentItems, sitemapPath, siteMapSize);
            
            return SitemapResult.Success(mapId, siteMapSize);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating sitemap");
            return SitemapResult.Fail("Unable to create sitemap due server error");
        }
    }

    private string GetPageUrl(ContentItemModel page, Site site)
    {
        var requestPath = $"{page.DestinationPath.Replace(Path.DirectorySeparatorChar, '/')}{page.FileName}";

        if (site.Mappings is not null)
        {
            var mappingPath = (
                from mapping in site.Mappings
                where mapping.Value.Equals(requestPath, StringComparison.InvariantCultureIgnoreCase)
                select mapping.Key).FirstOrDefault();

            requestPath = string.IsNullOrEmpty(mappingPath) ? requestPath : mappingPath;
        }

        return $"{_contentHostUrl}/{Uri.EscapeDataString(site.Name)}/{requestPath}";
    }

    private async Task<string> CreateOrUpdateContentItemAsync(Site site, IEnumerable<ContentItemModel> contentItems, string sitemapPath, decimal siteMapSize)
    {
        var existingSiteMap = contentItems.FirstOrDefault(i => i.FileName == SITEMAP_FILE_NAME);
            
        if (existingSiteMap is null)
        {
            var sitemapItem = new ContentItem()
            {
                Site = new Site { Id = site.Id, Name = site.Name },
                ContentType = XML_CONTENT_TYPE,
                FullName = sitemapPath,
                Name = SITEMAP_FILE_NAME,
                UploadedAt = DateTime.UtcNow,
                Size = siteMapSize
            };
            
            await _contentItemsCollection.InsertOneAsync(sitemapItem);

            return sitemapItem.Id;
        }

        await _contentItemsCollection.UpdateOneAsync(i => i.Id == existingSiteMap.Id,
            new UpdateDefinitionBuilder<ContentItem>()
                .Set(i => i.UpdateDate, DateTime.UtcNow)
                .Set(i => i.Size, siteMapSize)
        );
            
        return existingSiteMap.Id;
    }

    private async Task<long> SerializeAndSaveSitemapAsync(string sitemapPath, UrlSet data)
    {
        var fileInfo = new FileInfo(sitemapPath);
        Stream fileStream = null;
        
        try
        {
            var serializer = new XmlSerializer(typeof(UrlSet));
            fileStream = fileInfo.OpenWrite();

            serializer.Serialize(
                fileStream,
                data
            );

            await fileStream.FlushAsync();
        }
        finally
        {
            if (fileStream is not null)
            {
                await fileStream.DisposeAsync();
            }
        }

        return fileInfo.Length;
    }
}