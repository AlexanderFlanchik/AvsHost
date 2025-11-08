using System.Collections.Generic;
using System.Threading.Tasks;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services.Databases;

public interface IDatabaseService
{
    /// <summary>
    /// Checks if database name specified is in use
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <returns>An async operation which returns true if database name is unique, otherwise false</returns>
    Task<bool> IsDatabaseNameUnique(string databaseName, string userId);
    
    /// <summary>
    /// Returns a list of all user databases
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>An async operation which returns a list with database names</returns>
    Task<IEnumerable<string>> GetUserDatabases(string userId);
}

public class DatabaseService(MongoEntityRepository repository) : IDatabaseService
{
    private readonly IMongoCollection<Site> _sites = repository.GetEntityCollection<Site>(GeneralConstants.SITES_COLLECTION);
    
    public async Task<bool> IsDatabaseNameUnique(string databaseName, string userId)
    {
        var query = await _sites.FindAsync(s => s.DatabaseName == databaseName && s.CreatedBy.Id != userId);
        
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<string>> GetUserDatabases(string userId)
    {
        var filter = Builders<Site>.Filter.Where(s => s.CreatedBy.Id == userId && s.DatabaseName != null);
        
        var query= await _sites.DistinctAsync(s => s.DatabaseName, filter);
        var results = await query.ToListAsync();
        
        return results;
    }
}