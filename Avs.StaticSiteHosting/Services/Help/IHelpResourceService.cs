using Avs.StaticSiteHosting.Models;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services
{
    public interface IHelpResourceService
    {
        Task<HelpResource> GetHelpResourceAsync(string name);
    }
}