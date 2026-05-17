using Avs.StaticSiteHosting.GenAiApi.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Avs.StaticSiteHosting.GenAiApi.Repositories;

public interface IVendorRepository
{
    Task<Vendor?> GetVendorAsync(string vendorCode);
    Task<IEnumerable<Vendor>> GetVendorsAsync();
    Task CreateVendorAsync(Vendor vendor);
}

public class VendorRepository(IMongoClient client) : BaseRepository(client), IVendorRepository
{
    public async Task<IEnumerable<Vendor>> GetVendorsAsync()
    {
        return await GetCollection<Vendor>().AsQueryable().ToListAsync();
    }

    public async Task<Vendor?> GetVendorAsync(string vendorCode)
    {
        var vendors = GetCollection<Vendor>();
        var vendorFilter = Builders<Vendor>.Filter.Where(v => v.Code == vendorCode);
        return await vendors.Find(vendorFilter).FirstOrDefaultAsync();
    }

    public async Task CreateVendorAsync(Vendor vendor)
    {
        await GetCollection<Vendor>().InsertOneAsync(vendor);
    }
}