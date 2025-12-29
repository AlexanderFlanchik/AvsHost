using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.Common;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using Avs.StaticSiteHosting.Web.Services.ContentManagement;
using Avs.StaticSiteHosting.Web.Services.EventLog;
using Avs.StaticSiteHosting.Web.Services.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using Avs.Messaging.Contracts;
using System.Collections.Generic;

namespace Avs.StaticSiteHosting.Web.Services.Sites
{
    public class SiteManagementService : ISiteManagementService
    {
        private readonly ISiteService _siteService;
        private readonly ICustomRouteHandlerService _customRouteHandlerService;
        private readonly IUserService _userService;
        private readonly IContentManager _contentManager;
        private readonly IEventLogsService _eventLogsService;
        private readonly IMessagePublisher _publishEndpoint;

        public SiteManagementService(
            ISiteService siteService,
            IUserService userService,
            IContentManager contentManager,
            IEventLogsService eventLogsService,
            IMessagePublisher publishEndpoint,
            ICustomRouteHandlerService customRouteHandlerService)
        {
            _siteService = siteService;
            _userService = userService;
            _contentManager = contentManager;
            _eventLogsService = eventLogsService;
            _publishEndpoint = publishEndpoint;
            _customRouteHandlerService = customRouteHandlerService;
        }

        public async Task<(CreateSiteResponseModel, Exception)> CreateSiteAndProcessContentAsync(SiteDetailsModel siteDetails, string userId)
        {
            if (await _siteService.CheckSiteNameUsedAsync(siteDetails.SiteName, null))
            {
                return (null, new ConflictException("This site name is already in use."));
            }

            var currentUser = await _userService.GetUserByIdAsync(userId);
            if (currentUser is null)
            {
                return (null, new BadRequestException("Cannot find user by ID provided."));
            }

            currentUser.Password = null;
            var siteData = new Site()
            {
                Name = siteDetails.SiteName,
                Description = siteDetails.Description,
                IsActive = siteDetails.IsActive,
                CreatedBy = currentUser,
                LaunchedOn = DateTime.UtcNow,
                Mappings = siteDetails.ResourceMappings,
                LandingPage = siteDetails.LandingPage,
                DatabaseName = siteDetails.DatabaseName,
                TagIds = siteDetails.TagIds?.Select(id => new EntityRef { Id = id }).ToArray(),
                CustomHandlerIds = siteDetails.CustomRouteHandlers?.Select(h => new EntityRef { Id = h.Id }).ToList() ?? [],
            };

            var newSite = await _siteService.CreateSiteAsync(siteData);
            var contentItems = (await _contentManager.ProcessSiteContentAsync(newSite, siteDetails.UploadSessionId))
                .ToArray();
                        
            var customRouteHandlers = new List<CustomRouteHandlerModel>();
            if (siteDetails.CustomRouteHandlers!.Count > 0)
            {
                foreach(var newHandler in siteDetails.CustomRouteHandlers)
                {
                    var newHandlerId = await _customRouteHandlerService.CreateCustomRouteHandlerAsync(new CreateCustomRouteHandlerRequest(
                        newSite.Id,
                        newHandler.Name,
                        newHandler.Method,
                        newHandler.Path,
                        newHandler.Body));
                    customRouteHandlers.Add(newHandler with { Id = newHandlerId });
                }

                await _siteService.UpdateHandlerReferencesAsync(newSite.Id, customRouteHandlers.Select(h => h.Id));
            }

            var newSiteResponse = new CreateSiteResponseModel()
            {
                SiteId = newSite.Id,
                Name = newSite.Name,
                Description = newSite.Description,
                IsActive = newSite.IsActive,
                CreatedBy = currentUser?.Name,
                LaunchedOn = DateTime.UtcNow,
                Mappings = siteDetails.ResourceMappings,
                LandingPage = siteDetails?.LandingPage,
                DatabaseName = siteDetails?.DatabaseName,
                TagIds = siteDetails.TagIds,
                Uploaded = contentItems,
                CustomRouteHandlers = customRouteHandlers
            };

            await _eventLogsService.InsertSiteEventAsync(newSiteResponse.SiteId, "Site Created", SiteEventType.Information,
                $"Site '{newSiteResponse.Name}' was created successfully.");

            return (newSiteResponse, null);
        }

        public async Task<(UpdateSiteResponseModel, Exception)> UpdateSiteAndProcessContent(string siteId, string userId, SiteDetailsModel siteDetails)
        {
            var currentUser = await _userService.GetUserByIdAsync(userId);
            var siteToUpdate = await _siteService.GetSiteByIdAsync(siteId);
            if (siteToUpdate is null)
            {
                return (null, new NotFoundException());
            }

            if (await _siteService.CheckSiteNameUsedAsync(siteDetails.SiteName, siteId))
            {
                return (null, new ConflictException("This site name is already in use."));
            }

            if (siteToUpdate.CreatedBy.Id != userId)
            {
                return (null, new UnauthorizedAccessException());
            }

            siteToUpdate.Name = siteDetails.SiteName;
            siteToUpdate.Description = siteDetails.Description;
            siteToUpdate.Mappings = siteDetails.ResourceMappings;
            siteToUpdate.LandingPage = siteDetails.LandingPage;
            siteToUpdate.DatabaseName = siteDetails.DatabaseName;
            siteToUpdate.TagIds = siteDetails.TagIds?.Select(id => new EntityRef { Id = id }).ToArray();

            var customRouteHandlers = new List<CustomRouteHandlerModel>();
            if (siteDetails.CustomRouteHandlers.Count > 0)
            {                
                var newHandlers = siteDetails.CustomRouteHandlers.Where(h => string.IsNullOrEmpty(h.Id)).ToList();
                
                foreach (var newHandler in newHandlers)
                {
                    var handlerId = await _customRouteHandlerService.CreateCustomRouteHandlerAsync(new CreateCustomRouteHandlerRequest(
                        siteToUpdate.Id,
                        newHandler.Name,
                        newHandler.Method,
                        newHandler.Path,
                        newHandler.Body));
                    customRouteHandlers.Add(newHandler with { Id = handlerId });
                }

                var existingHandlers = siteDetails.CustomRouteHandlers.Where(h => !string.IsNullOrEmpty(h.Id)).ToList();
                foreach (var existingHandler in existingHandlers)
                {
                    await _customRouteHandlerService.UpdateCustomRouteHandlerAsync(existingHandler);
                    customRouteHandlers.Add(existingHandler);
                }

                var handlersToDelete = siteToUpdate.CustomRouteHandlers
                    .Where(h => !siteDetails.CustomRouteHandlers.Any(sh => sh.Id == h.Id))
                    .ToList();

                await _customRouteHandlerService.DeleteCustomRouteHandlersAsync(handlersToDelete.Select(h => h.Id));
                siteToUpdate.CustomHandlerIds = customRouteHandlers.Select(h => new EntityRef { Id = h.Id }).ToList();
            }
            else
            {
                // If no handlers provided, remove all existing handlers for the site
                await _customRouteHandlerService.DeleteCustomRouteHandlersAsync(siteToUpdate.CustomRouteHandlers.Select(h => h.Id));
            }

            bool siteStateChanged = siteToUpdate.IsActive != siteDetails.IsActive;
            if (siteStateChanged)
            {
                siteToUpdate.IsActive = siteDetails.IsActive;
                if (siteToUpdate.IsActive)
                {
                    await _eventLogsService.InsertSiteEventAsync(siteToUpdate.Id, "Site Started.", SiteEventType.Information,
                        $"Site '{siteToUpdate.Name}' was started by {currentUser.Name}.");
                }
                else
                {
                    await _eventLogsService.InsertSiteEventAsync(siteToUpdate.Id, "Site Stopped.", SiteEventType.Warning,
                         $"Site '{siteToUpdate.Name}' was stopped by {currentUser.Name}.");
                }
            }

            await _siteService.UpdateSiteAsync(siteToUpdate);

            var siteFileList = (await _contentManager.GetUploadedContentAsync(siteId)).ToList();
            if (!string.IsNullOrEmpty(siteDetails.UploadSessionId))
            {
                siteFileList.AddRange(await _contentManager.ProcessSiteContentAsync(siteToUpdate, siteDetails.UploadSessionId));
            }

            await _publishEndpoint.PublishAsync(new ContentUpdatedEvent { SiteId = siteId });

            return (new UpdateSiteResponseModel { Uploaded = siteFileList.ToArray(), CustomRouteHandlers = customRouteHandlers }, null);
        }
    }
}