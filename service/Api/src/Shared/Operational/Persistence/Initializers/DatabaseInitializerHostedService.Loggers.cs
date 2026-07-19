namespace Shared.Operational.Persistence.Initializers;

public static partial class DatabaseInitializerHostedServiceLoggers
{
    [LoggerMessage(
        EventId = 265,
        Level = LogLevel.Information,
        Message = "Database initialization complete.")]
    public static partial void InitializationComplete(ILogger logger);

    [LoggerMessage(
        EventId = 266,
        Level = LogLevel.Critical,
        Message = "Database initialization failed.")]
    public static partial void InitializationFailed(ILogger logger, Exception ex);
}
