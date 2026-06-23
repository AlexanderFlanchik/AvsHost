using Avs.StaticSiteHosting.GenAiApi.DTOs;
using Avs.StaticSiteHosting.GenAiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.GenAiApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ModelController(IModelService modelService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetModels([FromQuery] GetModelQuery query)
        => Ok(await modelService.GetModelsAsync(query));

    [HttpPost]
    public async Task<IActionResult> CreateModel(ModelData model)
    {
        var result = await modelService.CreateModelAsync(model);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }
    
    [HttpPut("{modelId}")]
    public async Task<IActionResult> UpdateModel(string modelId, ModelData model)
    {
        var updateResult = await modelService.UpdateModelAsync(new UpdateModelRequest(modelId, model));
        return updateResult.IsSuccess ? NoContent() : BadRequest(updateResult.Error);
    }

    [HttpDelete("{modelId}")]
    public async Task<IActionResult> DeleteModel(string modelId)
    {
        await  modelService.DeleteModelAsync(modelId);
        return NoContent();
    }
}