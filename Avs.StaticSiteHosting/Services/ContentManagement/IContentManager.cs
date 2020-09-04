using Avs.StaticSiteHosting.DTOs;
using Avs.StaticSiteHosting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Services.ContentManagement
{
    public interface IContentManager
    {
        Task CreateSiteContentAsync(Site site, string uploadSessionId);
        Task<IEnumerable<ContentItemModel>> GetUploadedContentAsync(string siteId);
    }
}
