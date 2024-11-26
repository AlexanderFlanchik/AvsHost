using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;
using Microsoft.Extensions.FileProviders;

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

app.MapCommonEndpoints();
app.MapSiteContent("/{sitename:required}/{**sitepath}");

app.Run();
