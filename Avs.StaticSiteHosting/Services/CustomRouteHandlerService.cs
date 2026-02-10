using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avs.Messaging.Contracts;
using Avs.StaticSiteHosting.Shared.Contracts;
using Avs.StaticSiteHosting.Web.DTOs;
using Avs.StaticSiteHosting.Web.Models;
using MongoDB.Driver;

namespace Avs.StaticSiteHosting.Web.Services;

public interface ICustomRouteHandlerService
{
    Task<string> CreateCustomRouteHandlerAsync(CreateCustomRouteHandlerRequest request);
    Task UpdateCustomRouteHandlerAsync(CustomRouteHandlerModel handlerToUpdate);
    Task<IEnumerable<CustomRouteHandlerModel>> GetCustomRouteHandlers(string siteId);
    Task DeleteCustomRouteHandlersAsync(IEnumerable<string> handlerIds);
}

public class CustomRouteHandlerService(MongoEntityRepository repository, IMessagePublisher publishEndpoint) : ICustomRouteHandlerService
{
    private readonly IMongoCollection<CustomRouteHandler> _handlers =
        repository.GetEntityCollection<CustomRouteHandler>(GeneralConstants.CUSTOM_ROUTE_HANDLER_COLLECTION);
    
    public async Task<string> CreateCustomRouteHandlerAsync(CreateCustomRouteHandlerRequest request)
    {
        var handlerEntity = new CustomRouteHandler()
        {
            SiteId = request.SiteId,
            Name = request.Name,
            Path = request.Path,
            Method = request.Method,
            Body = request.Body
        };
        
        await _handlers.InsertOneAsync(handlerEntity);
        await publishEndpoint.PublishAsync(
            new CustomRouteHandlerChanged()
            {
                HandlerId = handlerEntity.Id,
                HandlerBody = handlerEntity.Body
            });

        return handlerEntity.Id;
    }

    public async Task UpdateCustomRouteHandlerAsync(CustomRouteHandlerModel handlerToUpdate)
    {
        var update = Builders<CustomRouteHandler>.Update
            .Set(h => h.Name, handlerToUpdate.Name)
            .Set(h => h.Method, handlerToUpdate.Method)
            .Set(h => h.Path, handlerToUpdate.Path)
            .Set(h => h.Body, handlerToUpdate.Body);

        var filter = Builders<CustomRouteHandler>.Filter.Eq(h => h.Id, handlerToUpdate.Id);

        await _handlers.UpdateOneAsync(filter, update);
        await publishEndpoint.PublishAsync(
            new CustomRouteHandlerChanged()
            {
                HandlerId = handlerToUpdate.Id,
                HandlerBody = handlerToUpdate.Body
            });
    }
    
    public async Task<IEnumerable<CustomRouteHandlerModel>> GetCustomRouteHandlers(string siteId)
    {
        var handlerList = await _handlers.Find(h => h.SiteId == siteId).ToListAsync();
        
        return handlerList.Select(h =>
            new CustomRouteHandlerModel(
                h.Id,
                h.SiteId,
                h.Name,
                h.Method, 
                h.Path,
                h.Body));
    }

    public async Task DeleteCustomRouteHandlersAsync(IEnumerable<string> handlerIds)
    {
        var idsToDelete = handlerIds.ToArray();
        var filter = Builders<CustomRouteHandler>.Filter.In(h => h.Id, idsToDelete);
        
        await _handlers.DeleteManyAsync(filter);
        await publishEndpoint.PublishAsync(new CustomRouteHandlersDeleted() { HandlerIds = idsToDelete });
    }
}