using Avs.StaticSiteHosting.Web.DTOs;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Settings
{
    public interface ISettingsManager
    {
        Task<AppSettingsModel> GetAsync(string key);
        Task UpdateOrAddAsync(string key, string value, string description = null);
    }
}