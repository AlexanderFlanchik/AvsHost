using MongoDB.Driver;

namespace Avs.StaticSiteHosting.GenAiApi.Repositories;

public abstract class BaseRepository(IMongoClient client)
{
    private readonly IMongoDatabase _database = client.GetDatabase("GenAiApi");
    
    protected IMongoCollection<T> GetCollection<T>()
    {
        return _database.GetCollection<T>(typeof(T).Name);
    }
}