using System.Reflection;
using System.Text.Json.Serialization;

using Module.Catalog;
using Module.Identity;
using Module.Inventory;
using Module.Location;
using Module.Ordering;
using Module.Payment;
using Module.Profile;
using Module.Shipping;

using ReSys.ServiceDefaults;

using Shared.Application;
using Shared.Governance;
using Shared.Observability;
using Shared.Operational;
using Shared.Operational.Persistence.Health;
using Shared.Operational.Persistence.Initializers;
using Shared.Performance;
using Shared.Security;


Assembly[] additionalAssemblies = [typeof(Module.IModuleMarker).Assembly];

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure: Serialize all enums as their string names across every API response
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Configure: Add service defaults for Observability, Resilience, and Service Discovery
builder.AddServiceDefaults();
builder.AddObservability();
builder.AddApplication(additionalAssemblies);
builder.AddGovernance(additionalAssemblies);
builder.AddPerformance();
builder.AddSecurity();
builder.AddOperational(additionalAssemblies);

// Configure: Add modular
builder.AddLocationModule();
builder.AddIdentityModule();
builder.AddProfilesModule();
builder.AddCatalogModule();
builder.AddInventoryModule();
builder.AddOrderingModule();
builder.AddPaymentModule();
builder.AddShippingModule();

// Initialize: Register database init state, hosted service, and health check
builder.Services.AddSingleton<IDatabaseInitializationState, DatabaseInitializationState>();
builder.Services.AddSingleton<IDatabaseInitializer, DatabaseInitializerService>();
builder.Services.AddHostedService<DatabaseInitializerHostedService>();
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseInitializationHealthCheck>("database_initialization", tags: new[] { "ready" });

WebApplication app = builder.Build();

// Initialize: Map default health/liveness endpoints
app.MapDefaultEndpoints();

app.UseGovernance();
app.UsePerformance();
app.UseSecurity();
app.UseRateLimiter();
app.UseOperational();
app.UseObservability();
app.UseApplication();

await app.RunAsync();
