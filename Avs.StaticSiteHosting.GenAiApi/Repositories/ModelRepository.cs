using Avs.StaticSiteHosting.GenAiApi.Models;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.GenAiApi.Repositories;

public interface IModelRepository
{
    Task<IEnumerable<Model>> GetAllModelsAsync();
    Task<Model?> GetModelByNameAndVendorCodeAsync(string modelName, string vendorCode);
    Task<IEnumerable<Model>> GetModelsAsync(int page, int pageSize);
    Task CreateModelAsync(Model model);
    Task UpdateModelAsync(Model model);
    Task DeleteModelAsync(string modelId);
}

public class ModelRepository(IMongoClient client) : BaseRepository(client), IModelRepository 
{
    public async Task<IEnumerable<Model>> GetAllModelsAsync()
    {
        return await GetCollection<Model>()
            .Find(m => m.IsActive)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Model>> GetModelsAsync(int page, int pageSize)
    {
        var filterDefinition = Builders<Model>.Filter.Where(m => m.IsActive);
        return await GetCollection<Model>()
            .Aggregate()
            .Match(filterDefinition)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Model?> GetModelByNameAndVendorCodeAsync(string modelName, string vendorCode)
    {
        var vendors = GetCollection<Vendor>();
        var models = GetCollection<Model>();
       
        var pipeline = models
                .Aggregate()
                .Lookup<Model, Vendor, VendorModel>(
                    foreignCollection: vendors,
                    localField: m => m.VendorId,
                    foreignField: v => v.Id,
                    @as: x => x.Vendor
                )
                .Unwind<VendorModel, VendorModel>(x => x.Vendor)
                .Match(x => x.Vendor.Code == vendorCode && x.IsActive && x.Name == modelName);
            
        return await pipeline.FirstOrDefaultAsync();
    }

    public async Task CreateModelAsync(Model model)
    {
        await GetCollection<Model>().InsertOneAsync(model);
    }

    public async Task UpdateModelAsync(Model model)
    {
        await GetCollection<Model>().FindOneAndReplaceAsync(m => m.VendorId == model.VendorId, model);
    }

    public async Task DeleteModelAsync(string modelId)
    {
        await GetCollection<Model>().DeleteOneAsync(m => m.Id == modelId);
             
        // await GetCollection<Model>().FindOneAndUpdateAsync(
        //    m => m.VendorId == modelId, 
        //    new UpdateDefinitionBuilder<Model>().Set(m => m.IsActive, false));
    }

    private class VendorModel : Model
    {
        public Vendor Vendor { get; set; } = null!;
    }
}