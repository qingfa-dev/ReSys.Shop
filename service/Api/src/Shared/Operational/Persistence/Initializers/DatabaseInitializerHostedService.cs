using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Shared.Operational.Persistence.Health;

namespace Shared.Operational.Persistence.Initializers;

public sealed class DatabaseInitializerHostedService(
    IHostApplicationLifetime lifetime,
    IDatabaseInitializationState state,
    IDatabaseInitializer initializer,
    ILogger<DatabaseInitializerHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await initializer.InitializeAsync(stoppingToken);
            state.MarkComplete();
            logger.LogInformation("Database initialization complete.");
        }
        catch (Exception ex)
        {
            state.MarkFailed(ex);
            logger.LogCritical(ex, "Database initialization failed.");
            lifetime.StopApplication();
        }
    }
}
