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

var stripeApiKey = builder.AddParameter("StripeApiKey", secret: true);
var stripeWebhookSecret = builder.AddParameter("StripeWebhookSecret", secret: true);

// ── API ─────────────────────────────────────────────────────────

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Api>(Services.Api)
    .WithReference(database)
    .WithReference(redis)
    .WithReference(embedding)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithOtlpExporter()
    .WithEnvironment("GatewayProviders__stripe__SecretKey", stripeApiKey)
    .WithEnvironment("GatewayProviders__stripe__WebhookSecret", stripeWebhookSecret);

// ── Stripe CLI (webhook forwarder) ──────────────────────────────

var stripeForwardUrl = ReferenceExpression.Create(
    $"{api.GetEndpoint("http")}/api/storefront/webhooks/stripe");

builder.AddContainer("stripe-cli", "docker.io/stripe/stripe-cli")
    .WithEnvironment("STRIPE_API_KEY", stripeApiKey)
    .WithArgs("listen",
        "--forward-to", stripeForwardUrl,
        "--events", "payment_intent.succeeded,payment_intent.payment_failed,payment_intent.canceled,charge.refunded,charge.dispute.created,payment_intent.processing,payment_intent.requires_action")
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
