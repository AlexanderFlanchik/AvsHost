using Avs.StaticSiteHosting.GenAiApi.DTOs;
using Avs.StaticSiteHosting.GenAiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.GenAiApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PayloadMappingController(
    IPayloadMappingService payloadMappingService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateMapping(PayloadMappingData request)
    {
        var newMappingIdResult = await payloadMappingService.CreateMappingAsync(request);
        
        return newMappingIdResult.IsSuccess ? 
            Ok(newMappingIdResult.Data) : 
            Conflict(newMappingIdResult.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMapping(string id, PayloadMappingData request)
    {
        var updateResult = await payloadMappingService.UpdateMappingAsync(id, request);
        
        return updateResult.IsSuccess ?
            NoContent() : 
            NotFound(updateResult.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMapping(string id)
    {
        await payloadMappingService.DeleteMappingAsync(id);
        return NoContent();
    }
}