var builder = DistributedApplication.CreateBuilder(args);

var databaseName = builder.Configuration["Parameters:database_name"];
if (string.IsNullOrEmpty(databaseName))
{
    throw new InvalidOperationException("Database name is required.");
}

var mongo = builder.AddMongoDB("mongo");
var mongoDb = mongo.AddDatabase(databaseName);

builder.AddProject<Projects.Avs_StaticSiteHosting_Web>("avs-staticsitehosting-web")
    .WithReference(mongoDb)
    .WaitFor(mongoDb);

builder.Build().Run();
