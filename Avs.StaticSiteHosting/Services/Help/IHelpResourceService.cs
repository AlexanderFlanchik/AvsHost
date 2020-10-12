using Avs.StaticSiteHosting.Web.Models;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services
{
    public interface IHelpResourceService
    {
        Task<HelpResource> GetHelpResourceAsync(string name);
    }
}