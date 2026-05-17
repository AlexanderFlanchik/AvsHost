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
    IPayloadMappingProvider mappingProvider,
    EventBusPubSub pubSub,
    IPayloadMappingRepository mappingRepository) : IPayloadMappingService
{
    public async Task<Result<string>> CreateMappingAsync(PayloadMappingData payloadMapping)
    {
        var mapping = new PayloadMapping()
        {
            VendorCode = payloadMapping.VendorCode,
            JsonContent = payloadMapping.JsonContent,
            Type = payloadMapping.PayloadMappingType
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
        var mappingsExist = mappingProvider.GetMappings(mappingId).Any();
        if (!mappingsExist)
        {
            return Result<string>.Fail("Mapping does not exist");
        }

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