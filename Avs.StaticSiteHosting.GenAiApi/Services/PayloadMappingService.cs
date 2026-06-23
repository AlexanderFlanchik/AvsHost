using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.DTOs;
using Avs.StaticSiteHosting.GenAiApi.Models;
using Avs.StaticSiteHosting.GenAiApi.Repositories;
using Avs.StaticSiteHosting.GenAiApi.Services.PayloadFormatters;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public interface IPayloadMappingService
{
    Task<Result<string>> CreateMappingAsync(PayloadMappingData payloadMapping);
    Task<Result> UpdateMappingAsync(string mappingId, PayloadMappingData payloadMapping);
    Task DeleteMappingAsync(string mappingId);
}

public class PayloadMappingService(
    EventBusPubSub pubSub,
    ILlmProvider llmProvider,
    IPayloadMappingRepository mappingRepository) : IPayloadMappingService
{
    public async Task<Result<string>> CreateMappingAsync(PayloadMappingData payloadMapping)
    {
        var model = llmProvider.GetModelByVendorAndName(payloadMapping.VendorCode, payloadMapping.Model);
        var mappingType = payloadMapping.PayloadMappingType;
        
        if (model is null)
        {
            return Result<string>.Fail(
                $"No model '{payloadMapping.Model}' found for vendor '{payloadMapping.VendorCode}'");
        }

        if (await mappingRepository.MappingExistsAsync(model.ModelId, mappingType))
        {
            return Result<string>.Fail(
                $"{mappingType.ToString()} mapping for model {model.VendorCode}/{model.Name} already exists.");
        }
        
        var mapping = new PayloadMapping()
        {
            VendorCode = payloadMapping.VendorCode,
            JsonContent = payloadMapping.JsonContent,
            Type = mappingType
        };
        
        await mappingRepository.CreateMappingAsync(mapping);
        await pubSub.PublishAsync(
                new MappingPayloadChanged(
                    mapping.Id,
                    mapping.VendorCode,
                    mapping.JsonContent,
                    mapping.Type)
                );

        return Result<string>.Success(mapping.Id);
    }

    public async Task<Result> UpdateMappingAsync(string mappingId, PayloadMappingData payloadMapping)
    {
        var mapping = new PayloadMapping()
        {
            Id = mappingId,
            JsonContent = payloadMapping.JsonContent,
            Type = payloadMapping.PayloadMappingType
        };

        await mappingRepository.UpdateMappingAsync(mappingId, mapping);
        await pubSub.PublishAsync(new MappingPayloadChanged(
            mappingId,
            mapping.VendorCode,
            mapping.JsonContent,
            mapping.Type));
        
        return Result.Success();
    }

    public async Task DeleteMappingAsync(string mappingId)
    {
        await mappingRepository.DeleteMappingAsync(mappingId);
        await pubSub.PublishAsync(new MappingDeleted(mappingId));
    }
}