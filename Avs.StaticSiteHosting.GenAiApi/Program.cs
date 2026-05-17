using Avs.StaticSiteHosting.GenAiApi.Common;
using Avs.StaticSiteHosting.GenAiApi.Middlewares;
using Microsoft.AspNetCore.DataProtection;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("mongo");
builder.AddRedisClient("avs-redis");
builder.Services.AddDataProtection()
    .PersistKeysToMongoDb(sp =>
        {
            var mongoDbClient = sp.GetRequiredService<IMongoClient>();
            return mongoDbClient.GetDatabase("GenAiApi");
        },
        "DataProtectionKeys"
    );
builder.Services.AddModelCatalog();
builder.Services.AddPayloadMappings();
builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseHttpsRedirection();
app.UseMiddleware<LlmRequestProxyMiddleware>();

app.MapGet("/", () => "Avs.StaticSiteHosting.GenAiApi is running");
app.MapControllers();

app.Run();