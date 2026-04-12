var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.LabelVerificationSystem_Api>("api");

builder.AddProject<Projects.LabelVerificationSystem_Web>("web")
       .WithReference(api)
       .WithExternalHttpEndpoints();

builder.Build().Run();
