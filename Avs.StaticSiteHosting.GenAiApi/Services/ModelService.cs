using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.DTOs;
using Avs.StaticSiteHosting.GenAiApi.Models;
using Avs.StaticSiteHosting.GenAiApi.Repositories;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public interface IModelService
{
    /// <summary>
    /// Returns a page with models available
    /// </summary>
    /// <param name="query">DTO with pagination data</param>
    Task<IEnumerable<ModelResponse>> GetModelsAsync(GetModelQuery query);

    /// <summary>
    /// Returns selected model using its name and vendor code or null if model is not found
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <param name="vendorCode">Vendor code</param>
    Task<ModelResponse?> GetModelAsync(string modelName, string vendorCode);
    
    /// <summary>
    /// Creates a model
    /// </summary>
    /// <param name="request">A DTO with required model data</param>
    Task<Result<string>> CreateModelAsync(ModelData request);

    /// <summary>
    /// Updates a model
    /// </summary>
    /// <param name="request">A DTO with required model data</param>
    Task<Result> UpdateModelAsync(UpdateModelRequest request);

    /// <summary>
    /// Makes model to be inactive
    /// </summary>
    /// <param name="modelId">Model ID</param>
    Task DeleteModelAsync(string modelId);
}

public class ModelService(IModelRepository modelRepository, IVendorRepository vendorRepository, EventBusPubSub pubSub) : IModelService
{
    public async Task<IEnumerable<ModelResponse>> GetModelsAsync(GetModelQuery query)
    {
        return (await modelRepository.GetModelsAsync(query.Page, query.PageSize))
                .Select(m => new ModelResponse(
                        m.Id, 
                        m.Name, 
                        m.VendorId,
                        m.Paths,
                    LlmFeatureMapper.MapFromModelFeatures(m.Features).ToArray())
            );
    }

    public async Task<ModelResponse?> GetModelAsync(string modelName, string vendorCode)
    {
        var model = await modelRepository.GetModelByNameAndVendorCodeAsync(modelName, vendorCode);
        
        return model is null ? null :
            new ModelResponse(model.Id, model.Name, model.VendorId, model.Paths,
                LlmFeatureMapper.MapFromModelFeatures(model.Features).ToArray());
    }

    public async Task<Result<string>> CreateModelAsync(ModelData request)
    {
        var vendor = await vendorRepository.GetVendorAsync(request.VendorCode);
        if (vendor is null)
        {
            return Result<string>.Fail($"No vendor {request.VendorCode} found");
        }
        
        var existingModel = await modelRepository.GetModelByNameAndVendorCodeAsync(request.Name, request.VendorCode);
        if (existingModel is not null)
        {
            return Result<string>.Fail($"Model with name '{request.Name}' already exists");
        }
        
        var newModel = new Model()
        {
            Name = request.Name,
            VendorId = vendor.Id,
            Paths = request.Paths,
            DefaultModelUrl = request.DefaultModelUrl,
            Features = LlmFeatureMapper.MapToModelLlmFeatures(request.Features).ToArray(),
            CustomSettings = request.CustomSettings,
            PayloadMappings = request.PayloadMappings ?? [],
            IsActive = true
        };
        
        await modelRepository.CreateModelAsync(newModel);
        await pubSub.PublishAsync(new ModelCreated(
                newModel.Id,
                vendor.Id,
                newModel.Name,
                newModel.Paths,
                newModel.DefaultModelUrl,
                newModel.CustomSettings,
                newModel.PayloadMappings));
        
        return Result<string>.Success(newModel.Id);
    }

    public async Task<Result> UpdateModelAsync(UpdateModelRequest request)
    {
        var existingModel = await modelRepository.GetModelByNameAndVendorCodeAsync(request.Model.Name, request.Model.VendorCode);
        if (existingModel is null)
        {
            return Result.Fail($"No model with name '{request.Model.VendorCode}/{request.Model.Name}' found");
        }

        if (existingModel.Id != request.Id)
        {
            return Result.Fail("Invalid request model ID, does not match its database value");
        }

        var updatedModel = new Model()
        {
            Id = request.Id,
            Name = existingModel.Name,
            VendorId = existingModel.VendorId,
            Paths = request.Model.Paths,
            DefaultModelUrl = existingModel.DefaultModelUrl,
            CustomSettings = request.Model.CustomSettings,
            IsActive = request.Model.IsActive,
            PayloadMappings = request.Model.PayloadMappings ?? [],
            Features = LlmFeatureMapper.MapToModelLlmFeatures(request.Model.Features).ToArray()
        };
        
        await modelRepository.UpdateModelAsync(updatedModel);
        await pubSub.PublishAsync(
            new ModelUpdated(
                updatedModel.Id,
                updatedModel.Paths,
                updatedModel.DefaultModelUrl,
                updatedModel.CustomSettings,
                updatedModel.PayloadMappings));
        
        return Result.Success();
    }

    public async Task DeleteModelAsync(string modelId)
    {
        await modelRepository.DeleteModelAsync(modelId);
        await pubSub.PublishAsync(new ModelDeleted(modelId));
    }
}