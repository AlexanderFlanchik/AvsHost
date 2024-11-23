using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddStaticSiteOptions(builder.Configuration);
builder.Services.AddCloudStorage();
builder.Services.AddSiteContent(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration, options =>
{
    options.AddConsumer<SiteContentUpdatedConsumer>();
    options.AddRequestClient<GetSiteContentRequestMessage>();
    options.AddRequestClient<CloudSettingsRequest>();
});

var app = builder.Build();

app.MapGet("/favicon.ico", () => Results.NotFound());
app.MapGet("/", () => "Content server is up and running...");
app.MapSiteContent("/{sitename:required}/{**sitepath}");

app.Run();
