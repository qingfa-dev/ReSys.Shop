using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Shared.Operational.Persistence.Initializers;

public sealed class DatabaseInitializerService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    IHostEnvironment environment) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        bool runMigrations = configuration.GetValue<bool>("DatabaseInitialization:RunMigrations");
        bool runSeeders = !environment.IsProduction();

        await serviceProvider.InitializeDatabaseAsync(runMigrations, runSeeders);
    }
}
