var builder = DistributedApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Parameters:database_name"];
if (string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("Database name is required.");
}

var dbUsername = builder.AddParameter("db-username");
var dbPassword = builder.AddParameter("db-password", secret: true);
var rbUsername = builder.AddParameter("rb-username");
var rbPassword = builder.AddParameter("rb-password", secret: true);

var mongo = builder.AddMongoDB("mongo", 27017, dbUsername, dbPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var mongoDb = mongo.AddDatabase(databaseName);
var rabbitMq = builder.AddRabbitMQ("AvsBroker", rbUsername, rbPassword,55720);

builder.AddProject<Projects.Avs_StaticSiteHosting_DataMigrator>("avs-staticsitehosting-datamigrator")
    .WithReference(mongoDb)
    .WaitFor(mongoDb);

var dashboard = builder.AddProject<Projects.Avs_StaticSiteHosting_Web>("avs-staticsitehosting-web")
     .WithReference(mongoDb)
     .WaitFor(mongoDb)
     .WithReference(rabbitMq)
     .WaitFor(rabbitMq);

builder.AddProject<Projects.Avs_StaticSiteHosting_ContentHost>("avs-staticsitehosting-contenthost")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(dashboard)
    .WaitFor(dashboard);

builder.Build().Run();
