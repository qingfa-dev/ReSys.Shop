using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Persistence;
using Module.Location.Persistence;
using Module.Ordering.Persistence;
using Module.Profile.Persistence;

using Npgsql;

using Respawn;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;
using Shared.Security.Identity.Domain.Shared;

using Testcontainers.PostgreSql;

namespace Api.Tests.Infrastructure;

public sealed class ApiFixture : IAsyncLifetime
{
    public static readonly IReadOnlyList<string> AllSchemas =
    [
        CatalogSchema.Name,
        IdentitySchema.Name,
        LocationSchema.Name,
        OrderingSchema.Name,
        ProfileSchema.Name
    ];

    private static readonly Lazy<Dictionary<string, Type[]>> _seedersBySchema = new(BuildSeederGroups);

    private PostgreSqlContainer? _container;
    private ApiFactory? _factory;
    private Respawner? _respawner;
    private ConcurrentDictionary<string, Respawner>? _respawnersBySchema;
    private string? _connectionString;

    public HttpClient Client => _factory?.CreateClient()
        ?? throw new InvalidOperationException("Factory not initialized");

    public ApiFactory Factory => _factory
        ?? throw new InvalidOperationException("Factory not initialized");

    public async ValueTask InitializeAsync()
    {
        ConfigureContainerRuntime();

        bool reuse = string.Equals(
            Environment.GetEnvironmentVariable("TESTCONTAINERS_REUSE_ENABLE"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        PostgreSqlBuilder builder = new PostgreSqlBuilder("pgvector/pgvector:pg17");
        if (reuse)
        {
            builder = builder.WithReuse(true);
        }

        _container = builder.Build();
        await _container.StartAsync();

        _connectionString = _container.GetConnectionString();

        _factory = new ApiFactory(_connectionString);

        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        IEnumerable<string> pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }

        _respawnersBySchema = new ConcurrentDictionary<string, Respawner>(StringComparer.OrdinalIgnoreCase);
        foreach (string schema in AllSchemas)
        {
            await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            Respawner perSchema = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                SchemasToInclude = [schema],
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
            _respawnersBySchema[schema] = perSchema;
        }

        await using (NpgsqlConnection allConn = new NpgsqlConnection(_connectionString))
        {
            await allConn.OpenAsync();
            _respawner = await Respawner.CreateAsync(allConn, new RespawnerOptions
            {
                SchemasToInclude = AllSchemas.ToArray(),
                TablesToIgnore = ["__EFMigrationsHistory"]
            });
        }

        await RunSeedersAsync(scope, AllSchemas);
    }

    public Task ResetDatabaseAsync() => ResetSchemasAsync(AllSchemas);

    public async Task ResetSchemasAsync(IEnumerable<string> schemas)
    {
        if (_respawnersBySchema is null || _connectionString is null)
            return;

        HashSet<string> schemaSet = new(schemas, StringComparer.OrdinalIgnoreCase);

        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (string schema in schemaSet)
        {
            if (_respawnersBySchema.TryGetValue(schema, out Respawner? respawner))
            {
                await respawner.ResetAsync(connection);
            }
        }

        using IServiceScope scope = _factory!.Services.CreateScope();
        await RunSeedersAsync(scope, schemaSet);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();

        if (_container is not null)
            await _container.DisposeAsync();
    }

    private static async Task RunSeedersAsync(IServiceScope scope, IEnumerable<string> schemas)
    {
        Dictionary<string, Type[]> seederGroups = _seedersBySchema.Value;
        HashSet<string> allowed = new(schemas, StringComparer.OrdinalIgnoreCase);

        HashSet<Type> allowedSeederTypes = seederGroups
            .Where(kvp => allowed.Contains(kvp.Key))
            .SelectMany(kvp => kvp.Value)
            .ToHashSet();

        IEnumerable<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>()
            .Where(s => allowedSeederTypes.Contains(s.GetType()));

        foreach (IDataSeeder seeder in seeders.OrderBy(s => s.Order))
        {
            try
            {
                await seeder.SeedAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seeder {seeder.GetType().Name} (Order {seeder.Order}) failed: {ex.Message}");
            }
        }
    }

    private static Dictionary<string, Type[]> BuildSeederGroups()
    {
        Dictionary<string, List<Type>> groups = new(StringComparer.OrdinalIgnoreCase)
        {
            [CatalogSchema.Name] = new List<Type>(),
            [IdentitySchema.Name] = new List<Type>(),
            [LocationSchema.Name] = new List<Type>(),
            [OrderingSchema.Name] = new List<Type>(),
            [ProfileSchema.Name] = new List<Type>(),
        };

        Type seederType = typeof(IDataSeeder);

        IEnumerable<Type> candidateTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(SafeGetTypes)
            .Where(t => seederType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (Type type in candidateTypes)
        {
            string? schema = ResolveSchemaFromNamespace(type.Namespace);
            if (schema is not null && groups.TryGetValue(schema, out List<Type>? list))
            {
                list.Add(type);
            }
        }

        return groups.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }

    private static string? ResolveSchemaFromNamespace(string? ns)
    {
        if (string.IsNullOrEmpty(ns))
            return null;

        if (ns.Contains(".Catalog.", StringComparison.OrdinalIgnoreCase))
            return CatalogSchema.Name;
        if (ns.Contains(".Location.", StringComparison.OrdinalIgnoreCase))
            return LocationSchema.Name;
        if (ns.Contains(".Ordering.", StringComparison.OrdinalIgnoreCase))
            return OrderingSchema.Name;
        if (ns.Contains(".Profile.", StringComparison.OrdinalIgnoreCase))
            return ProfileSchema.Name;
        if (ns.Contains(".Identity.", StringComparison.OrdinalIgnoreCase) ||
            ns.Contains("Security.Identity", StringComparison.OrdinalIgnoreCase))
            return IdentitySchema.Name;

        return null;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null).Cast<Type>();
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
            string? xdgRuntimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            if (xdgRuntimeDir is not null)
            {
                string podmanSocket = Path.Combine(xdgRuntimeDir, "podman", "podman.sock");
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
