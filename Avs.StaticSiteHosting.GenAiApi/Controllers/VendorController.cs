using Avs.StaticSiteHosting.GenAiApi.DTOs;
using Avs.StaticSiteHosting.GenAiApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avs.StaticSiteHosting.GenAiApi.Controllers;

[Route(("api/[controller]"))]
[ApiController]
public class VendorController(IVendorService vendorService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await vendorService.GetAllVendorsAsync());

    [HttpPost]
    public async Task<IActionResult> CreateVendor(CreateVendorRequest request)
    {
        var vendorResult = await vendorService.CreateVendorAsync(request);
        
        return vendorResult.IsSuccess ? Ok(vendorResult.Data) : BadRequest(vendorResult.Error);
    }
}