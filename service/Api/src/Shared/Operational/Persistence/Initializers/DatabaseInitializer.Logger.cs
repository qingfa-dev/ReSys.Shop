namespace Shared.Operational.Persistence.Initializers;

public static partial class DatabaseInitializer
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 248,
            Level = LogLevel.Information,
            Message = "Starting database initialization...")]
        public static partial void LogInitializationStarted(ILogger logger);

        [LoggerMessage(
            EventId = 249,
            Level = LogLevel.Information,
            Message = "Applying migrations for {DbContext}...")]
        public static partial void LogApplyingMigrations(ILogger logger, string dbContext);

        [LoggerMessage(
            EventId = 250,
            Level = LogLevel.Information,
            Message = "Migration attempt {Attempt} of {MaxAttempts}...")]
        public static partial void LogMigrationAttempt(ILogger logger, int attempt, int maxAttempts);

        [LoggerMessage(
            EventId = 251,
            Level = LogLevel.Information,
            Message = "Migration succeeded on attempt {Attempt}.")]
        public static partial void LogMigrationSuccess(ILogger logger, int attempt);

        [LoggerMessage(
            EventId = 252,
            Level = LogLevel.Error,
            Message = "Migration failed on attempt {Attempt}: {ExceptionType} - {Message}")]
        public static partial void LogMigrationFailed(ILogger logger, int attempt, string exceptionType, string message);

        [LoggerMessage(
            EventId = 253,
            Level = LogLevel.Warning,
            Message = "Migration attempt {Attempt} failed, retrying in {DelayMs}ms: {Message}")]
        public static partial void LogMigrationRetry(ILogger logger, int attempt, int delayMs, string message);

        [LoggerMessage(
            EventId = 254,
            Level = LogLevel.Information,
            Message = "Migrations applied successfully.")]
        public static partial void LogMigrationsApplied(ILogger logger);

        [LoggerMessage(
            EventId = 255,
            Level = LogLevel.Warning,
            Message = "Migration execution failed for {DbContext} after all retries.")]
        public static partial void LogMigrationExecutionFailed(ILogger logger, string dbContext);

        [LoggerMessage(
            EventId = 257,
            Level = LogLevel.Information,
            Message = "Running {SeederCount} database seeders...")]
        public static partial void LogRunningSeeders(ILogger logger, int seederCount);

        [LoggerMessage(
            EventId = 258,
            Level = LogLevel.Information,
            Message = "No data seeders found.")]
        public static partial void LogNoSeedersFound(ILogger logger);

        [LoggerMessage(
            EventId = 259,
            Level = LogLevel.Information,
            Message = "Running seeder: {SeederName}")]
        public static partial void LogRunningSeeder(ILogger logger, string seederName);

        [LoggerMessage(
            EventId = 260,
            Level = LogLevel.Error,
            Message = "Seeder {SeederName} failed: {Errors}")]
        public static partial void LogSeederFailed(ILogger logger, string seederName, string errors);

        [LoggerMessage(
            EventId = 261,
            Level = LogLevel.Information,
            Message = "Seeder {SeederName} completed successfully.")]
        public static partial void LogSeederCompleted(ILogger logger, string seederName);

        [LoggerMessage(
            EventId = 262,
            Level = LogLevel.Error,
            Message = "Seeder {SeederName} threw an exception: {ExceptionType}: {Message}")]
        public static partial void LogSeederException(ILogger logger, string seederName, string exceptionType, string message);

        [LoggerMessage(
            EventId = 263,
            Level = LogLevel.Information,
            Message = "Seeding finished. Successful: {Successful}, Failed: {Failed}.")]
        public static partial void LogSeedingFinished(ILogger logger, int successful, int failed);

        [LoggerMessage(
            EventId = 264,
            Level = LogLevel.Critical,
            Message = "A fatal error occurred during database initialization.")]
        public static partial void LogFatalError(ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = 265,
            Level = LogLevel.Information,
            Message = "Migrations skipped (runMigrations=false).")]
        public static partial void LogMigrationsSkipped(ILogger logger);

        [LoggerMessage(
            EventId = 266,
            Level = LogLevel.Information,
            Message = "Seeders skipped (runSeeders=false).")]
        public static partial void LogSeedersSkipped(ILogger logger);
    }
}
