using Avs.Messaging;
using Avs.Messaging.RabbitMq;
using Avs.StaticSiteHosting.ContentHost.Common;
using Avs.StaticSiteHosting.ContentHost.Messaging.SiteContent;
using Avs.StaticSiteHosting.Shared.Common;
using Avs.StaticSiteHosting.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddStaticSiteOptions(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddCloudStorage();
builder.Services.AddSiteContent(builder.Configuration);
builder.Services.AddMessaging(x =>
{
    x.AddConsumer<SiteContentUpdatedConsumer>();
    x.AddConsumer<ClearSiteCacheConsumer>();
    x.AddConsumer<SiteContentInfoResponseConsumer>();
    x.AddConsumer<CloudSettingsResponseConsumer>();
    
    x.UseRabbitMq(cfg =>
    {
        var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQSettings").Get<RabbitMqSettings>()!;
        cfg.Host = rabbitMqSettings.Host;
        cfg.Port = rabbitMqSettings.Port;
        
        cfg.AddRpcClient();
        cfg.ConfigureRequestReply<GetSiteContentRequestMessage, SiteContentInfoResponse>();
        cfg.ConfigureRequestReply<CloudSettingsRequest, CloudSettingsResponse>();
        cfg.ConfigureExchangeOptions<SiteVisited>(o => o.ExchangeName = "SiteVisited");
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapCommonEndpoints();
app.MapSiteContent("/{sitename:required}/{**sitepath}");

app.Run();
