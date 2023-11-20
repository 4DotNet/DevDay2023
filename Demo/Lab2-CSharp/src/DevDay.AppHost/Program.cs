var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.DevDay_ApiService>("apiservice");
builder.AddProject<Projects.DevDay_Web>("webfrontend")
    .WithReference(apiService);

builder.Build().Run();
