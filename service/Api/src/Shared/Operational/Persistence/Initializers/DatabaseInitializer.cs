using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;

namespace Shared.Operational.Persistence.Initializers;

public static partial class DatabaseInitializer
{
    private static class ResilienceConfig
    {
        public const int MaxRetryAttempts = 5;
        public const int InitialDelayMs = 3000;
        public const double BackoffMultiplier = 2.0;
    }

    public static async Task InitializeDatabaseAsync(
        this WebApplication app,
        bool runMigrations = true,
        bool runSeeders = true)
    {
        await app.Services.InitializeDatabaseAsync(runMigrations, runSeeders);
    }

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        bool runMigrations = true,
        bool runSeeders = true)
    {
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();
        ILogger logger = loggerFactory.CreateLogger(typeof(DatabaseInitializer).FullName!);

        Loggers.LogInitializationStarted(logger);

        try
        {
            using IServiceScope scope = services.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            bool dropSchemas = configuration.GetValue<bool>("DatabaseInitialization:DropSchemas");

            if (dropSchemas)
            {
                await DropApplicationSchemasAsync(scope, logger);
            }

            // Ensure the pgvector extension exists before applying migrations
        await EnsureVectorExtensionAsync(scope, logger);

            if (runMigrations)
            {
                await RunMigrationsAsync(scope, logger);
            }
            else
            {
                Loggers.LogMigrationsSkipped(logger);
            }

            // Per-model HNSW vector indexes — created after migrations because
            // EF Core cannot generate expression indexes with dimension casts.
            await EnsureVectorIndexesAsync(scope, logger);

            if (runSeeders)
            {
                await RunSeedersAsync(scope, logger);
            }
            else
            {
                Loggers.LogSeedersSkipped(logger);
            }
        }
        catch (Exception ex)
        {
            Loggers.LogFatalError(logger, ex);
            throw;
        }
    }

    private static async Task DropApplicationSchemasAsync(IServiceScope scope, ILogger logger)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        if (dbContext is not DbContext efContext) return;

        Loggers.LogDroppingSchemas(logger);

        var schemas = new[] { "catalog", "inventory", "location", "payment", "shipping", "ordering", "profile", "identity" };
        foreach (var schema in schemas)
        {
            try
            {
                await efContext.Database.ExecuteSqlAsync(
                    FormattableStringFactory.Create($"DROP SCHEMA IF EXISTS {schema} CASCADE"));
            }
            catch (Exception ex)
            {
                Loggers.LogSchemaDropFailed(logger, schema, ex.Message);
            }
        }

        // Also clear the EF migrations history so the new InitialCreate runs cleanly
        try
        {
            await efContext.Database.ExecuteSqlAsync(
                FormattableStringFactory.Create("DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE"));
        }
        catch { /* table may not exist */ }

        Loggers.LogSchemasDropped(logger);
    }

    private static async Task EnsureVectorExtensionAsync(IServiceScope scope, ILogger logger)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        if (dbContext is not DbContext efContext) return;

        try
        {
            await efContext.Database.ExecuteSqlRawAsync(
                "CREATE EXTENSION IF NOT EXISTS vector");
            Loggers.LogVectorExtensionEnsured(logger);
        }
        catch (Exception ex)
        {
            Loggers.LogVectorExtensionFailed(logger, ex.Message);
            throw new InvalidOperationException(
                "pgvector extension is required but could not be created. " +
                "Ensure the PostgreSQL instance has pgvector installed (use pgvector/pgvector Docker image).", ex);
        }
    }

    private static async Task EnsureVectorIndexesAsync(IServiceScope scope, ILogger logger)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        if (dbContext is not DbContext efContext) return;

        // Per-model HNSW partial indexes on the untyped vector column.
        // pgvector requires expression indexes with dimension casts (::vector(dim))
        // for untyped vector columns — EF Core can't generate these.
        (string Slug, int Dim)[] models =
        [
            ("fashion_clip", 512),
            ("clip_b32", 512),
            ("clip_l14", 768),
            ("clip_vit_b16", 512),
            ("clip_generic", 512),
            ("dinov2_vits14", 384),
            ("convnext_tiny", 768),
            ("siglip", 768),
            ("eva_clip", 512),
            ("efficientnet_b0", 1280),
            ("resnet50", 2048),
        ];

        try
        {
            foreach (var (slug, dim) in models)
            {
                var indexName = $"ix_product_image_embeddings_vector_hnsw_{slug}";
                await efContext.Database.ExecuteSqlRawAsync(
                    "CREATE INDEX IF NOT EXISTS \"{0}\" " +
                    "ON catalog.variant_image_embeddings " +
                    "USING hnsw ((vector::vector({1})) vector_cosine_ops) " +
                    "WHERE model_name = '{2}'", indexName, dim, slug);
            }

            logger.LogInformation("Ensured {Count} per-model HNSW vector indexes", models.Length);
        }
        catch (Exception ex)
        {
            // Non-fatal: indexes improve query performance but aren't required for correctness
            logger.LogWarning(ex, "Failed to ensure per-model HNSW vector indexes: {Message}", ex.Message);
        }
    }

    private static async Task RunMigrationsAsync(IServiceScope scope, ILogger logger)
    {
        IEnumerable<IApplicationDbContext> contexts = scope.ServiceProvider.GetServices<IApplicationDbContext>();

        Task[] migrationTasks = contexts
            .OfType<DbContext>()
            .Select(dbContext => ApplyOneContextAsync(dbContext, logger))
            .ToArray();

        await Task.WhenAll(migrationTasks);
    }

    private static async Task ApplyOneContextAsync(DbContext dbContext, ILogger logger)
    {
        string dbContextName = dbContext.GetType().Name;
        Loggers.LogApplyingMigrations(logger, dbContextName);

        bool migrationApplied = await ApplyMigrationsWithRetryAsync(
            dbContext,
            logger);

        if (!migrationApplied)
        {
            Loggers.LogMigrationExecutionFailed(logger, dbContextName);
        }
        else
        {
            Loggers.LogMigrationsApplied(logger);
        }
    }

    private static async Task<bool> ApplyMigrationsWithRetryAsync(
        DbContext context,
        ILogger logger)
    {
        int attemptNumber = 0;
        int delayMs = ResilienceConfig.InitialDelayMs;

        while (attemptNumber < ResilienceConfig.MaxRetryAttempts)
        {
            try
            {
                attemptNumber++;

                Loggers.LogMigrationAttempt(logger, attemptNumber, ResilienceConfig.MaxRetryAttempts);

                await context.Database.MigrateAsync(CancellationToken.None);

                Loggers.LogMigrationSuccess(logger, attemptNumber);
                return true;
            }
            catch (Exception migrationEx)
            {
                bool isTransient = IsTransientException(migrationEx);

                if (attemptNumber >= ResilienceConfig.MaxRetryAttempts || !isTransient)
                {
                    Loggers.LogMigrationFailed(
                        logger,
                        attemptNumber,
                        migrationEx.GetType().Name,
                        migrationEx.Message);

                    return false;
                }

                Loggers.LogMigrationRetry(logger, attemptNumber, delayMs, migrationEx.Message);

                await Task.Delay(delayMs);
                delayMs = (int)(delayMs * ResilienceConfig.BackoffMultiplier);
            }
        }

        return false;
    }

    private static async Task RunSeedersAsync(IServiceScope scope, ILogger logger)
    {

        List<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToList();

        if (seeders.Count == 0)
        {
            Loggers.LogNoSeedersFound(logger);
            return;
        }

        Loggers.LogRunningSeeders(logger, seeders.Count);

        int successfulSeeders = 0;
        int failedSeeders = 0;

        foreach (IDataSeeder seeder in seeders.OrderBy(s => s.Order))
        {
            string seederName = seeder.GetType().Name;
            Loggers.LogRunningSeeder(logger, seederName);

            try
            {
                Result result = await seeder.SeedAsync(CancellationToken.None);

                if (result.IsFailure)
                {
                    failedSeeders++;
                    if (logger.IsEnabled(LogLevel.Error))
                    {
                        string errorMessages = string.Join(", ", result.Errors.Select(e => e.Message));
                        Loggers.LogSeederFailed(logger, seederName, errorMessages);
                    }
                }
                else
                {
                    successfulSeeders++;
                    Loggers.LogSeederCompleted(logger, seederName);
                }
            }
            catch (Exception seederEx)
            {
                failedSeeders++;
                Loggers.LogSeederException(
                    logger,
                    seederName,
                    seederEx.GetType().Name,
                    seederEx.Message);

                var dbContext = scope.ServiceProvider.GetService<IApplicationDbContext>();
                if (dbContext is DbContext efContext)
                {
                    efContext.ChangeTracker.Clear();
                }
            }
        }

        Loggers.LogSeedingFinished(logger, successfulSeeders, failedSeeders);
    }

    private static bool IsTransientException(Exception exception)
    {
        if (exception is TimeoutException)
            return true;

        if (exception is InvalidOperationException &&
            (exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
             exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase)))
            return true;

        if (exception is PostgresException)
            return false;

        if (exception is NpgsqlException)
            return true;

        if (exception.InnerException is Exception inner)
            return IsTransientException(inner);

        return false;
    }
}
