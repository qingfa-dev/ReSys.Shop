namespace Module.Ordering.Backgrounds;

public sealed partial class CartExpiryJob
{
    public static partial class Loggers
    {
        [LoggerMessage(
            EventId = 5001,
            Level = LogLevel.Debug,
            Message = "[CartExpiry.Found]: {Count} draft carts past cutoff {Cutoff}")]
        public static partial void Found(ILogger logger, int Count, DateTimeOffset Cutoff);

        [LoggerMessage(
            EventId = 5002,
            Level = LogLevel.Information,
            Message = "[CartExpiry.Completed]: Expired {Count} draft carts")]
        public static partial void Completed(ILogger logger, int Count);

        [LoggerMessage(
            EventId = 5003,
            Level = LogLevel.Information,
            Message = "[CartExpiry.Scheduler]: Registered recurring job {JobId} with cron {Cron}")]
        public static partial void SchedulerRegistered(ILogger logger, string JobId, string Cron);
    }
}