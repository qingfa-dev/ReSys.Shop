using System.Reflection;

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
using Shared.Operational.Persistence.Initializers;
using Shared.Performance;
using Shared.Security;

Assembly[] additionalAssemblies = [typeof(Module.IModuleMarker).Assembly];

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure: Add service defaults for Observability, Resilience, and Service Discovery
builder.AddServiceDefaults();
builder.AddObservability();
builder.AddApplication(additionalAssemblies);
builder.AddGovernance(additionalAssemblies);
builder.AddPerformance();
builder.AddSecurity();
builder.AddOperational(additionalAssemblies);

// Configure: Add moudular
builder.AddLocationModule();
builder.AddIdentityModule();
builder.AddProfilesModule();
builder.AddCatalogModule();
builder.AddInventoryModule();
builder.Services.AddOrderingModule();
builder.Services.AddPaymentModule(builder.Configuration);
builder.Services.AddShippingModule();

WebApplication app = builder.Build();

// Initialize: Map default health/liveness endpoints
app.MapDefaultEndpoints();

app.UseGovernance();
app.UsePerformance();
app.UseSecurity();
app.UseRateLimiter();
app.UseOperational();
app.UseObservability();
app.UseHttpsRedirection();
app.UseApplication();

// Initialize: Apply pending migrations and run seeders before accepting traffic
bool runSeeders = !app.Environment.IsProduction();
await app.InitializeDatabaseAsync(runMigrations: true, runSeeders: runSeeders);

await app.RunAsync();
