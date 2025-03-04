var builder = DistributedApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Parameters:database_name"];
if (string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("Database name is required.");
}

var mongo = builder.AddMongoDB("mongo");
var mongoDb = mongo.AddDatabase(databaseName);
var rabbitMq = builder.AddRabbitMQ("avs_broker");

builder.AddProject<Projects.Avs_StaticSiteHosting_DataMigrator>("avs-staticsitehosting-datamigrator")
    .WithReference(mongoDb)
    .WaitFor(mongoDb);

builder.AddProject<Projects.Avs_StaticSiteHosting_Web>("avs-staticsitehosting-web")
     .WithReference(mongoDb)
     .WaitFor(mongoDb)
     .WithReference(rabbitMq)
     .WaitFor(rabbitMq);

builder.AddProject<Projects.Avs_StaticSiteHosting_ContentHost>("avs-staticsitehosting-contenthost")
     .WithReference(rabbitMq)
     .WaitFor(rabbitMq);

builder.Build().Run();
