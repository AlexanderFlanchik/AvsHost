using System;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.ContentManagement;

public interface IPagePreviewSessionService
{
    Task StartPreviewSessionAsync(string previewSessionId, string htmlTreeJson);
    Task<string> GetHtmlTreeAsync(string previewSessionId);
}

public class PagePreviewSessionService(MongoEntityRepository repository) : IPagePreviewSessionService
{
    private readonly IMongoCollection<PagePreviewEntity> _pagePreviews =
        repository.GetEntityCollection<PagePreviewEntity>(GeneralConstants.PAGE_PREVIEW_COLLECTION);

    public async Task StartPreviewSessionAsync(string previewSessionId, string htmlTreeJson)
    {
        var existingPreview = await _pagePreviews.Find(x => x.PreviewSessionId == previewSessionId).ToListAsync();
        if (existingPreview.Count > 0)
        {
            var entity = existingPreview[0];
            entity.HtmlTreeJson = htmlTreeJson;
            entity.Timestamp = DateTime.UtcNow;
            await _pagePreviews.ReplaceOneAsync(x => x.PreviewSessionId == previewSessionId, entity);
        }
        else
        {
            var newPreview = new PagePreviewEntity()
            {
                PreviewSessionId = previewSessionId,
                HtmlTreeJson = htmlTreeJson,
                Timestamp = DateTime.UtcNow
            };
            
            await _pagePreviews.InsertOneAsync(newPreview);            
        }
    }

    public async Task<string> GetHtmlTreeAsync(string previewSessionId)
    {
        var filter = Builders<PagePreviewEntity>.Filter.Eq(x => x.PreviewSessionId, previewSessionId);
        var preview = await _pagePreviews.Find(filter).ToListAsync();
        
        return preview.Count > 0 ? preview[0].HtmlTreeJson : null;
    }
}