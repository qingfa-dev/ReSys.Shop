using System.Reflection;

using Module.Location;

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
builder.AddLocationsModule();

WebApplication app = builder.Build();

// Initialize: Map default health/liveness endpoints
app.MapDefaultEndpoints();

app.UseGovernance();
app.UsePerformance();
app.UseSecurity();
app.UseOperational();
app.UseObservability();
app.UseHttpsRedirection();
app.UseApplication();

// Initialize: Apply pending migrations and run seeders before accepting traffic
bool runSeeders = !app.Environment.IsProduction();
await app.InitializeDatabaseAsync(runMigrations: true, runSeeders: runSeeders);

await app.RunAsync();
