var builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ContainerResource> sqlServer = builder
    .AddContainer(name: "azuresqledge", 
        image: "mcr.microsoft.com/azure-sql-edge")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.TalkLikeTv_Mvc>("talkliketv-mvc")
    .WaitFor(sqlServer);

builder.Build().Run();