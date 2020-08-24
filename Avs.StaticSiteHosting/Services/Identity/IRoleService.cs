using Avs.StaticSiteHosting.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services.Identity
{
    public interface IRoleService
    {
        Task<Role> GetRoleByNameAsync(string roleName);
    }
}
