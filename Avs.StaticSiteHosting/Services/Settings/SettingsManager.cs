using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IMongoCollection<AppSettings> _settingsCollection;
        
        public SettingsManager(MongoEntityRepository repository)
        {
            _settingsCollection = repository.GetEntityCollection<AppSettings>(GeneralConstants.APP_SETTINGS_COLLECTION);
        }

        public async Task<AppSettingsModel> GetAsync(string key)
        {
            var query =
                await _settingsCollection.FindAsync(s => s.Name == key);
            var setting = await query.FirstOrDefaultAsync();
            if (setting is null)
            {
                return null;
            }

            return new AppSettingsModel 
            { 
                Id = setting.Id, 
                Name = key, 
                Value = setting.Value, 
                Description = setting.Description 
            };
        }

        public async Task UpdateOrAddAsync(string key, string value, string description = null)
        {
            var query =
                await _settingsCollection.FindAsync(s => s.Name == key);
            var setting = await query.FirstOrDefaultAsync();
            if (setting is null)
            {
                var newSetting = new AppSettings()
                {
                    Name = key,
                    Value = value,
                    Description = description
                };

                await _settingsCollection.InsertOneAsync(newSetting);
            }
            else
            {
                var updateBuilder = new UpdateDefinitionBuilder<AppSettings>();
                var update = updateBuilder.Set(k => k.Value, value).Set(k => k.Description, description);

                await _settingsCollection.UpdateOneAsync(s => s.Name == key, update);
            }
        }
    }
}