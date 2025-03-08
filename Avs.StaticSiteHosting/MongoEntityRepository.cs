using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web
{
    /// <summary>
    /// General repository which servers collections of Mongo entities.
    /// </summary>
    public class MongoEntityRepository
    {
        private readonly IMongoDatabase _database;

        public MongoEntityRepository(IMongoClient client)
        {
            _database = client.GetDatabase("StaticSiteDb");
        }

        public IMongoCollection<T> GetEntityCollection<T>(string collectionName)
        {
            var collection = _database.GetCollection<T>(collectionName);

            return collection;
        }
    }
}