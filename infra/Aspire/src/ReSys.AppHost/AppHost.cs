using Microsoft.Extensions.Configuration;

using ReSys.ServiceDefaults.Constants;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// Load this AppHost project's user-secrets (UserSecretsId in the csproj) so the
// Stripe key can be configured once via `dotnet user-secrets` instead of a shell export.
builder.Configuration.AddUserSecrets<Program>();

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
    .WithHttpEndpoint(targetPort: 8000, name: "http");


IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis)
    .WithReference(embedding)
    .WithEnvironment("Http__Clients__Inference__BaseAddress", embedding.GetEndpoint("http"))
    .WithHttpHealthCheck("/health")
    .WithHttpsEndpoint(port: 5001, name: "https")
    .WithExternalHttpEndpoints()
    .WithOtlpExporter()
    .WaitFor(redis)
    .WaitFor(postgres);

// Resolve the Stripe key from user-secrets ("Stripe:ApiKey") with the process
// env var as a fallback, so both `dotnet user-secrets set` and shell export work.
var stripeApiKey = builder.Configuration["Stripe:ApiKey"]
    ?? Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
if (!string.IsNullOrEmpty(stripeApiKey))
{
    // The forward URL must be resolved lazily by Aspire at runtime — interpolating
    // api.GetEndpoint("https") directly yields the EndpointReference type name.
    builder.AddExecutable("stripe-listen", "stripe", Environment.CurrentDirectory)
        .WithEnvironment("STRIPE_API_KEY", stripeApiKey)
        .WithArgs("listen", "--forward-to",
            api.GetEndpoint("https") + "/api/storefront/billing/webhooks/stripe")
        .WaitFor(api);
    Console.WriteLine("[stripe] stripe-listen resource added (forwarding to https://localhost:5001/api/storefront/billing/webhooks/stripe).");
}
else
{
    Console.WriteLine("[stripe] WARNING: STRIPE_SECRET_KEY is not set in this process; the 'stripe-listen' webhook resource will NOT be created.");
    Console.WriteLine("[stripe]   Export it in the terminal that runs this AppHost, then restart: export STRIPE_SECRET_KEY=sk_test_...");
}

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
