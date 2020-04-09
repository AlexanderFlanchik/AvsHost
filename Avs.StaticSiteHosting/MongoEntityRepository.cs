using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting
{
    /// <summary>
    /// General repository which servers collections of Mongo entities.
    /// </summary>
    public class MongoEntityRepository
    {
        private readonly MongoDbSettings settings;

        public MongoEntityRepository(IOptions<MongoDbSettings> mongoDbOptions)
        {
            settings = mongoDbOptions.Value;            
        }

        public IMongoCollection<T> GetEntityCollection<T>(string collectionName)
        {
            var mongoClient = new MongoClient(settings.Host);
            var db = mongoClient.GetDatabase(settings.Database);
            var collection = db.GetCollection<T>(collectionName);

            return collection;
        }
    }
}