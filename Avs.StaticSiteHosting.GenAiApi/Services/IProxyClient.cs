namespace Avs.StaticSiteHosting.GenAiApi.Services;

public interface IProxyClient
{
    /// <summary>
    /// Proxies LLM request to vendor
    /// </summary>
    /// <param name="context">current <see cref="HttpContext"/> instance</param>
    /// <param name="vendor">Vendor settings</param>
    /// <param name="model">Model settings</param>
    /// <returns>A task which resolves when request is handled</returns>
    Task ProxyLlmRequestAsync(HttpContext context, VendorSettings vendor, ModelSettings model);
}