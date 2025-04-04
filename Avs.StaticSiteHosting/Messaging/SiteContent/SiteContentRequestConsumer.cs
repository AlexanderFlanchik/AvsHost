﻿using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.Models.Identity;
using Avs.StaticSiteHosting.Web.Services;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using MassTransit;
using System.Linq;
using System.Threading.Tasks;

namespace Avs.StaticSiteHosting.Web.Messaging.SiteContent
{
    public class SiteContentRequestConsumer(ISiteService siteService, IContentManager contentManager) : IConsumer<GetSiteContentRequestMessage>
    {
        public async Task Consume(ConsumeContext<GetSiteContentRequestMessage> context)
        {
            var siteInfo = await siteService.GetSiteByNameAsync(context.Message.SiteName);
            if (siteInfo is null)
            {
                await context.RespondAsync(new SiteContentInfoResponse());

                return;
            }

            var siteContent = new SiteContentInfo()
            {
                Id = siteInfo.Id,
                Name = siteInfo.Name,
                Description = siteInfo.Description,
                IsActive = siteInfo.IsActive,
                LaunchedOn = siteInfo.LaunchedOn,
                LastStopped = siteInfo.LastStopped,
                LandingPage = siteInfo.LandingPage,
                Mappings = siteInfo.Mappings,
                User = new UserDetails
                {
                    Id = siteInfo.CreatedBy.Id,
                    UserName = siteInfo.CreatedBy.Name,
                    IsActive = siteInfo.CreatedBy.Status == UserStatus.Active
                },
                ContentItems = (await contentManager.GetUploadedContentAsync(siteInfo.Id))
                    .Select(i => new ContentItemInfo
                    {
                        Id = i.Id,
                        FileName = i.FileName,
                        DestinationPath = i.DestinationPath,
                        ContentType = i.ContentType,
                        CacheDuration = i.CacheDuration
                    }).ToArray(),
            };

            await context.RespondAsync(new SiteContentInfoResponse { SiteContent = siteContent });
        }
    }
}