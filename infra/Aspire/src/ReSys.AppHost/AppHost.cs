using ReSys.ServiceDefaults.Constants;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

// ── Infrastructure ──────────────────────────────────────────────

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

// ── Stripe secrets ──────────────────────────────────────────────

var stripeApiKey = builder.AddParameter(Stripe.Parameters.ApiKey, secret: true);
var stripeWebhookSecret = builder.AddParameter(Stripe.Parameters.WebhookSecret, secret: true);

// ── API ─────────────────────────────────────────────────────────

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis)
    .WithReference(embedding)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithOtlpExporter()
    .WithEnvironment(Stripe.EnvironmentVariables.GatewaySecretKey, stripeApiKey)
    .WithEnvironment(Stripe.EnvironmentVariables.GatewayWebhookSecret, stripeWebhookSecret);

// ── Stripe CLI (webhook forwarder) ──────────────────────────────

var stripeForwardUrl = ReferenceExpression.Create(
    $"{api.GetEndpoint("http")}{Stripe.WebhookRoute}");

builder.AddContainer(Services.StripeCli, Images.Stripe.Cli)
    .WithEnvironment(Stripe.EnvironmentVariables.ApiKey, stripeApiKey)
    .WithArgs(Stripe.Cli.Command,
        Stripe.Cli.ForwardTo, stripeForwardUrl,
        Stripe.Cli.Events, Stripe.WebhookEvents)
    .WaitFor(api);

// ── Admin SPA ───────────────────────────────────────────────────

#pragma warning disable ASPIRECERTIFICATES001
builder.AddViteApp(Application.Admin, "../../../../app/Admin")
    .WithPnpm()
    .WithReference(api)
    .WithHttpsEndpoint(port: 5173, env: "PORT")
    .WithHttpsDeveloperCertificate()
    .WithDeveloperCertificateTrust(trust: true);
#pragma warning restore ASPIRECERTIFICATES001

// ── Store SPA ───────────────────────────────────────────────────

#pragma warning disable ASPIRECERTIFICATES001
builder.AddViteApp(Application.Store, "../../../../app/Store")
    .WithPnpm()
    .WithReference(api)
    .WithHttpsEndpoint(port: 5174, env: "PORT")
    .WithHttpsDeveloperCertificate()
    .WithDeveloperCertificateTrust(trust: true);
#pragma warning restore ASPIRECERTIFICATES001

builder.Build().Run();
