using System.Collections.Concurrent;
using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Models;
using Avs.StaticSiteHosting.GenAiApi.Repositories;
using Microsoft.AspNetCore.DataProtection;

namespace Avs.StaticSiteHosting.GenAiApi.Services;

public interface ILlmProvider : IEventSubscriber
{
    /// <summary>
    /// Gets all available vendor settings (with models)
    /// </summary>
    IEnumerable<VendorSettings> GetAllVendors();
    
    /// <summary>
    /// Gets vendor settings by Vendor Id
    /// </summary>
    /// <param name="vendorId">Vendor ID</param>
    /// <returns>Vendor settings if found or null otherwise</returns>
    VendorSettings? GetVendorById(string vendorId);
    
    /// <summary>
    /// Gets a model by name and vendor
    /// </summary>
    /// <param name="vendorCode">Vendor code (e.g. openai)</param>
    /// <param name="modelName">Model name</param>
    /// <returns>An instance of <see cref="ModelSettings"/> if found, null otherwise</returns>
    ModelSettings? GetModelByVendorAndName(string vendorCode, string modelName);

    /// <summary>
    /// Gets models of all available vendors
    /// </summary>
    IEnumerable<ModelSettings> GetAllModels();
    
    /// <summary>
    /// Loads model catalog data from the database. Should be run ance at application startup.
    /// </summary>
    Task LoadDataAsync();
}

public class LlmProvider(
    IServiceScopeFactory scopeFactory,
    IDataProtectionProvider protectionProvider,
    EventBusPubSub pubSub,
    ILogger<LlmProvider> logger) : ILlmProvider
{
    private readonly ConcurrentDictionary<string, VendorSettings> _vendorSettings = new();
    private readonly IDataProtector _protector = protectionProvider.CreateProtector(AppConstants.ApiKeyProtector);
    
    public async Task LoadDataAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var vendorRepository = scope.ServiceProvider.GetRequiredService<IVendorRepository>();
        var modelRepository = scope.ServiceProvider.GetRequiredService<IModelRepository>();

        Vendor[] vendors = [];
        Model[] models = [];

        Task[] dataTasks = [
            Task.Run(async () => vendors = (await vendorRepository.GetVendorsAsync()).ToArray()),
            Task.Run(async () => models = (await modelRepository.GetAllModelsAsync()).ToArray())
        ];
        
        await Task.WhenAll(dataTasks);

        foreach (var vendor in vendors)
        {
            var vendorModels = models.Where(m => m.VendorId == vendor.Id)
                .Select(m =>
                {
                    var settings = new ModelSettings
                    {
                        VendorId = vendor.Id,
                        ModelId = m.Id,
                        VendorCode = vendor.Code,
                        Name = m.Name,
                        Paths = m.Paths,
                    };
                    
                    settings.SetPayloadMappings(m.PayloadMappings);
                    
                    return settings;
                })
                .ToList();

            var vendorSettings = new VendorSettings(vendorModels)
            {
                VendorId = vendor.Id,
                Code = vendor.Code,
                BaseUrl = vendor.BaseUrl,
                ApiKey = _protector.Unprotect(vendor.ApiKey)
            };
            
            _vendorSettings.AddOrUpdate(vendor.Id, vendorSettings,  (_, __) => vendorSettings);
        }
        
        logger.LogInformation("Found {vendorCount} vendor(s), {modelCount} model(s)", vendors.Length, models.Length);
    }
    
    public IEnumerable<VendorSettings> GetAllVendors() => _vendorSettings.Values;
    
    public VendorSettings? GetVendorById(string vendorId)
    {
        return _vendorSettings.GetValueOrDefault(vendorId);
    }

    public ModelSettings? GetModelByVendorAndName(string vendorCode, string modelName)
    {
        var vendor = _vendorSettings.Values.FirstOrDefault(v => v.Code == vendorCode);
        if (vendor is null)
        {
            return null;
        }

        var model = _vendorSettings.Values
            .Where(v => v.VendorId == vendor.VendorId)
            .SelectMany(v => v.Models)
            .FirstOrDefault(m => m.Name == modelName);
        
        return model;
    }

    public IEnumerable<ModelSettings> GetAllModels() => _vendorSettings.Values.SelectMany(m => m.Models);

    #region ModelCatalog Events

    public async Task SubscribeToEventsAsync()
    {
        await pubSub.SubscribeAsync<VendorCreated>(On);
        await pubSub.SubscribeAsync<VendorUpdated>(On);
        await pubSub.SubscribeAsync<ModelCreated>(On);
        await pubSub.SubscribeAsync<ModelUpdated>(On);
        await pubSub.SubscribeAsync<ModelDeleted>(On);
    }
    
    private void On(VendorCreated @event)
    {
        if (_vendorSettings.TryGetValue(@event.VendorId, out _))
        {
            return;
        }

        var newVendor = new VendorSettings(new List<ModelSettings>())
        {
            Code = @event.VendorCode,
            BaseUrl = @event.BaseUrl,
            ApiKey = @event.ApiKey,
            CustomSettings = @event.CustomSettings
        };
        
        _vendorSettings.TryAdd(@event.VendorId, newVendor);
    }
    
    private void On(VendorUpdated @event)
    {
        if (!_vendorSettings.TryGetValue(@event.VendorId, out var vendorToUpdate))
        {
            return;
        }

        lock (vendorToUpdate)
        {
            vendorToUpdate.ApiKey = @event.ApiKey;
            vendorToUpdate.BaseUrl = @event.BaseUrl;
            vendorToUpdate.CustomSettings = @event.CustomSettings;
        }
    }

    private void On(ModelCreated @event)
    {
        if (!_vendorSettings.TryGetValue(@event.VendorId, out var vendor))
        {
            return;
        }

        var model = new ModelSettings()
        {
            ModelId = @event.ModelId,
            VendorCode = vendor.Code,
            Name = @event.Name,
            Paths = @event.Paths,
            DefaultModelUrl = @event.DefaultModelUrl,
            CustomSettings = @event.CustomSettings,
        };
        
        model.SetPayloadMappings(@event.PayloadMappings);

        lock (vendor)
        {
            vendor.Models.Add(model);
        }
    }

    private void On(ModelUpdated @event)
    {
        var modelToUpdate = GetAllModels().FirstOrDefault(m => m.ModelId == @event.ModelId);
        if (modelToUpdate is null)
        {
            return;
        }

        lock (modelToUpdate)
        {
            modelToUpdate.Paths = @event.Paths;
            modelToUpdate.SetPayloadMappings(@event.PayloadMappings);
            modelToUpdate.DefaultModelUrl = @event.DefaultModelUrl;
            modelToUpdate.CustomSettings = @event.CustomSettings;
        }
    }

    private void On(ModelDeleted @event)
    {
        var modelToDelete = GetAllModels().FirstOrDefault(m => m.ModelId == @event.ModelId);
        if (modelToDelete is null)
        {
            return;
        }

        var vendor = _vendorSettings.Values.FirstOrDefault(v => v.Code == modelToDelete.VendorCode);
        if (vendor is null)
        {
            return;
        }

        lock (vendor)
        {
            vendor.Models.Remove(modelToDelete);
        }
    }
    
    #endregion
}