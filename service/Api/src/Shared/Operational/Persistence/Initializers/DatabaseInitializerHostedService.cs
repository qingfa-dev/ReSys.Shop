using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Shared.Operational.Persistence.Health;

namespace Shared.Operational.Persistence.Initializers;

/// <summary>Hosted service that runs database initialization (migrations + seeders) on application startup.</summary>
public sealed class DatabaseInitializerHostedService(
    IHostApplicationLifetime lifetime,
    IDatabaseInitializationState state,
    IDatabaseInitializer initializer,
    ILogger<DatabaseInitializerHostedService> logger) : BackgroundService
{
    /// <summary>Executes database initialization — stops the application on critical failure.</summary>
    /// <param name="stoppingToken">Cancellation token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await initializer.InitializeAsync(stoppingToken);
            state.MarkComplete();
            DatabaseInitializerHostedServiceLoggers.InitializationComplete(logger);
        }
        catch (Exception ex)
        {
            state.MarkFailed(ex);
            DatabaseInitializerHostedServiceLoggers.InitializationFailed(logger, ex);
            lifetime.StopApplication();
        }
    }
}
