using Avs.StaticSiteHosting.Web.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Services.Identity
{
    public interface IRoleService
    {
        Task<Role> GetRoleByNameAsync(string roleName);
    }
}
