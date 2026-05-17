namespace Avs.StaticSiteHosting.GenAiApi.Services;

public interface IProxyClient
{
    /// <summary>
    /// Proxies LLM request to vendor
    /// </summary>
    /// <param name="vendor">Vendor settings</param>
    /// <param name="model">Model settings</param>
    /// <param name="requestBodyContent">JSON request payload</param>
    /// <param name="context">current <see cref="HttpContext"/> instance</param>
    /// <returns>A task which resolves when request is handled</returns>
    Task ProxyLlmRequestAsync(VendorSettings vendor, ModelSettings model, string requestBodyContent, HttpContext  context);
}