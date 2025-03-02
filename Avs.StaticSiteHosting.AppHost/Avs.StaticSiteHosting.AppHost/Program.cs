var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Avs_StaticSiteHosting_Web>("avs-staticsitehosting-web");

builder.Build().Run();
