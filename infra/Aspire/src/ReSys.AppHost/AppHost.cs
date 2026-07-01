using Aspire.Hosting.JavaScript;

using ReSys.ServiceDefaults.Constants;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres(Infrastructures.Databases.Server)
    .WithImage(Images.Pgvector.Optimized);

IResourceBuilder<RedisResource> redis = builder.AddRedis(Infrastructures.Cache.Resource)
    .WithImage(Images.Redis.Optimized);

IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase(
    Infrastructures.Databases.Resource);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis);

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
