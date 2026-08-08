using ReSys.ServiceDefaults.Constants;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres(Infrastructures.Databases.Server)
    .WithImage(Images.Pgvector.Optimized)
    .WithPgAdmin();

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
    .WithHttpEndpoint(targetPort: 8000, name: "http");


IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis)
    .WithReference(embedding)
    .WithEnvironment("Http__Clients__Inference__BaseAddress", embedding.GetEndpoint("http"))
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithOtlpExporter();

#pragma warning disable ASPIRECERTIFICATES001
builder.AddViteApp(Application.Admin, "../../../../app/Admin")
    .WithPnpm()
    .WithReference(api)
    .WithHttpsEndpoint(port: 5173, env: "PORT")
    .WithHttpsDeveloperCertificate()
    .WithDeveloperCertificateTrust(trust: true);
#pragma warning restore ASPIRECERTIFICATES001

#pragma warning disable ASPIRECERTIFICATES001
builder.AddViteApp(Application.Store, "../../../../app/Store")
    .WithPnpm()
    .WithReference(api)
    .WithHttpsEndpoint(port: 5174, env: "PORT")
    .WithHttpsDeveloperCertificate()
    .WithDeveloperCertificateTrust(trust: true);
#pragma warning restore ASPIRECERTIFICATES001
builder.Build().Run();
