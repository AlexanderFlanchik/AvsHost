using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web
{
    /// <summary>
    /// General repository which servers collections of Mongo entities.
    /// </summary>
    public class MongoEntityRepository
    {
        private readonly IMongoDatabase database;

        public MongoEntityRepository(IOptions<MongoDbSettings> mongoDbOptions)
        {
            var settings = mongoDbOptions.Value;
            var mongoClient = new MongoClient(settings.Host);
            database = mongoClient.GetDatabase(settings.Database);
        }

        public IMongoCollection<T> GetEntityCollection<T>(string collectionName)
        {
            var collection = database.GetCollection<T>(collectionName);

            return collection;
        }
    }
}