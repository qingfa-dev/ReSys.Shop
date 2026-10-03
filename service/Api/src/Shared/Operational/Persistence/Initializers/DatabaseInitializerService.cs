using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Shared.Operational.Persistence.Initializers;

/// <summary>Initializes the database on application startup — runs migrations and optionally seeds data in non-production environments.</summary>
public sealed class DatabaseInitializerService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IHostEnvironment environment) : IDatabaseInitializer
{
    /// <summary>Applies pending EF Core migrations and runs seeders when not in production.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Validate: Read config to decide whether to run migrations
        bool runMigrations = configuration.GetValue<bool>("DatabaseInitialization:RunMigrations");
        // Validate: Only seed in non-production environments to avoid polluting prod data
        bool runSeeders = !environment.IsProduction();

        // Call: Delegate to the EF Core database initializer extension
        await serviceProvider.InitializeDatabaseAsync(runMigrations, runSeeders);
    }
}
