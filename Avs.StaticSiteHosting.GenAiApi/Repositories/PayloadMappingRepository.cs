using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Avs.StaticSiteHosting.GenAiApi.Repositories;

public interface IPayloadMappingRepository
{
    Task<bool> MappingExistsAsync(string modelId, PayloadMappingType mappingType);
    Task<PayloadMapping?> GetMappingAsync(string mappingId);
    Task<IEnumerable<PayloadMapping>> GetAllMappingsAsync();
    Task CreateMappingAsync(PayloadMapping payloadMapping);
    Task UpdateMappingAsync(string mappingId, PayloadMapping payloadMapping);
    Task DeleteMappingAsync(string mappingId);
}

public class PayloadMappingRepository(IMongoClient client) : BaseRepository(client), IPayloadMappingRepository
{
    public async Task<bool> MappingExistsAsync(string modelId, PayloadMappingType mappingType)
        => await GetCollection<PayloadMapping>().Find(x => x.ModelId == modelId && x.Type == mappingType).AnyAsync();

    public async Task<PayloadMapping?> GetMappingAsync(string mappingId)
       => await GetCollection<PayloadMapping>().Find(x => x.Id == mappingId).FirstOrDefaultAsync();

    public async Task<IEnumerable<PayloadMapping>> GetAllMappingsAsync()
        => await GetCollection<PayloadMapping>().AsQueryable().ToListAsync();
    
    public Task CreateMappingAsync(PayloadMapping payloadMapping)
        => GetCollection<PayloadMapping>().InsertOneAsync(payloadMapping);

    public Task UpdateMappingAsync(string mappingId, PayloadMapping payloadMapping)
        => GetCollection<PayloadMapping>().ReplaceOneAsync(x => x.Id == mappingId, payloadMapping);

    public Task DeleteMappingAsync(string mappingId)
        => GetCollection<PayloadMapping>().DeleteOneAsync(x => x.Id == mappingId);
}