using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
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

            if (runMigrations)
            {
                await RunMigrationsAsync(scope, logger);
            }
            else
            {
                Loggers.LogMigrationsSkipped(logger);
            }

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

    private static async Task RunMigrationsAsync(IServiceScope scope, ILogger logger)
    {
        IEnumerable<IApplicationDbContext> contexts = scope.ServiceProvider.GetServices<IApplicationDbContext>();

        foreach (IApplicationDbContext context in contexts)
        {
            if (context is not DbContext dbContext)
                continue;

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
