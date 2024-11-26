using Avs.StaticSiteHosting.ContentHost.Properties;

namespace Avs.StaticSiteHosting.ContentHost.Services;

public interface IErrorPageHandler
{
    Task Handle(HttpContext context, string errorMessage);
}

public class ErrorPageHandler : IErrorPageHandler
{
    public async Task Handle(HttpContext context, string errorMessage)
    {
        string errorPageContent = string.Format(Resources.ErrorPageTemplate, errorMessage);
        
        await context.Response.WriteAsync(errorPageContent);
    }
}