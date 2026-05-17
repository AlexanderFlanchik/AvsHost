using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.DTOs;
using Avs.StaticSiteHosting.GenAiApi.Models;
using Avs.StaticSiteHosting.GenAiApi.Repositories;
using Microsoft.AspNetCore.DataProtection;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public interface IVendorService
{
    /// <summary>
    /// Gets all list of vendors supported
    /// </summary>
    Task<IEnumerable<VendorResponse>> GetAllVendorsAsync();
    
    /// <summary>
    /// Creates a vendor
    /// </summary>
    /// <param name="request">New vendor data</param>
    /// <returns>Vendor ID</returns>
    Task<Result<string>> CreateVendorAsync(CreateVendorRequest request);
}

public class VendorService(
    IVendorRepository repository,
    EventBusPubSub pubSub,
    IDataProtectionProvider dataProtectionProvider)
    : IVendorService 
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector(AppConstants.ApiKeyProtector);
    
    public async Task<IEnumerable<VendorResponse>> GetAllVendorsAsync()
    {
        return (await repository.GetVendorsAsync())
            .Select(v => new VendorResponse(v.Id, v.Code, v.Name,  v.BaseUrl, v.Description, v.CustomSettings));
    }

    public async Task<Result<string>> CreateVendorAsync(CreateVendorRequest request)
    {
        var existingVendor = await repository.GetVendorAsync(request.Code);
        if (existingVendor is not null)
        {
            return Result<string>.Fail($"Vendor \"{existingVendor.Code} ({existingVendor.Name})\" already exists.");
        }

        var vendor = new Vendor()
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            BaseUrl = request.BaseUrl,
            CustomSettings = request.CustomSettings,
            ApiKey = _protector.Protect(request.ApiKey)
        };
        
        await repository.CreateVendorAsync(vendor);
        await pubSub.PublishAsync(new VendorCreated(vendor.Id, vendor.Code, vendor.BaseUrl, request.ApiKey, vendor.CustomSettings));
        
        return Result<string>.Success(vendor.Id);
    }
}