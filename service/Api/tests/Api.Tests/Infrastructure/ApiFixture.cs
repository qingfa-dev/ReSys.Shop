using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Respawn;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;

using Testcontainers.PostgreSql;

namespace Api.Tests.Infrastructure;

public sealed class ApiFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private ApiFactory? _factory;
    private Respawner? _respawner;
    private string? _connectionString;

    public HttpClient Client => _factory?.CreateClient()
        ?? throw new InvalidOperationException("Factory not initialized");

    public async ValueTask InitializeAsync()
    {
        ConfigureContainerRuntime();

        _container = new PostgreSqlBuilder("pgvector/pgvector:pg17")
            .Build();

        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        _factory = new ApiFactory(_connectionString);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        await RunSeedersAsync(scope);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            SchemasToInclude = ["locations", "profiles", "identity"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null || _connectionString is null)
            return;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);

        using var scope = _factory!.Services.CreateScope();
        await RunSeedersAsync(scope);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        if (_container is not null)
            await _container.DisposeAsync();
    }

    private static async Task RunSeedersAsync(IServiceScope scope)
    {
        IEnumerable<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>();

        foreach (IDataSeeder seeder in seeders.OrderBy(s => s.Order))
        {
            await seeder.SeedAsync(CancellationToken.None);
        }
    }

    private static void ConfigureContainerRuntime()
    {
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOCKER_HOST")))
            return;

        string? socketPath = null;

        if (File.Exists("/var/run/docker.sock"))
        {
            socketPath = "/var/run/docker.sock";
        }
        else
        {
            var xdgRuntimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            if (xdgRuntimeDir is not null)
            {
                var podmanSocket = Path.Combine(xdgRuntimeDir, "podman", "podman.sock");
                if (File.Exists(podmanSocket))
                    socketPath = podmanSocket;
            }
        }

        if (socketPath is null)
            return;

        Environment.SetEnvironmentVariable("DOCKER_HOST", $"unix://{socketPath}");
        Environment.SetEnvironmentVariable("TESTCONTAINERS_DOCKER_SOCKET_OVERRIDE", socketPath);
        Environment.SetEnvironmentVariable("TESTCONTAINERS_RYUK_CONTAINER_PRIVILEGED", "true");
    }
}
