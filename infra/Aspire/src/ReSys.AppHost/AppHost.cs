using Aspire.Hosting.JavaScript;

using ReSys.ServiceDefaults.Constants;

// [WIP-MVP] YARP API gateway is deferred to v1.x. The Services.Gateway constant is defined
// in ReSys.ServiceDefaults but not registered as a resource here. Frontends call the API
// directly via VITE_API_URL. See docs/superpowers/specs/2026-07-07-mvp-cut-design.md.

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres(Infrastructures.Databases.Server)
    .WithImage(Images.Pgvector.Optimized);

IResourceBuilder<RedisResource> redis = builder.AddRedis(Infrastructures.Cache.Resource)
    .WithImage(Images.Redis.Optimized);

IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase(
    Infrastructures.Databases.Resource);

var embedding = builder.AddUvicornApp(
        Services.Embedding,
        "../../../../service/Embedding",
        "embedding.main:app")
    .WithUv()
    .WithHttpHealthCheck("/health")
    .WithHttpEndpoint(targetPort: 8000, name: "http")
    .WithExternalHttpEndpoints();

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis)
    .WithReference(embedding)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

IResourceBuilder<ViteAppResource> store = builder.AddViteApp(Application.Store, "../../../../app/Store")
    .WithPnpm()
    .WithHttpEndpoint(targetPort: 5173)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api);

IResourceBuilder<ViteAppResource> admin = builder.AddViteApp(Application.Admin, "../../../../app/Admin")
    .WithPnpm()
    .WithHttpEndpoint(targetPort: 5174)
    .WithEnvironment("VITE_API_URL", api.GetEndpoint("http"))
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
