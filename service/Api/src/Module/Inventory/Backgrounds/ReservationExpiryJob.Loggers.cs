namespace Module.Inventory.Backgrounds;

public static partial class ReservationExpiryJobLoggers
{
    [LoggerMessage(
        EventId = 5201,
        Level = LogLevel.Information,
        Message = "[ReservationExpiry.SweepCompleted]: Expired {Count} stock reservations and restored stock")]
    public static partial void SweepCompleted(ILogger logger, int Count);

    [LoggerMessage(
        EventId = 5202,
        Level = LogLevel.Error,
        Message = "[ReservationExpiry.SweepFailed]: {Message}")]
    public static partial void SweepFailed(ILogger logger, string Message);

    [LoggerMessage(
        EventId = 5203,
        Level = LogLevel.Information,
        Message = "[ReservationExpiry.Scheduler]: Registered recurring job {JobId} with cron {Cron}")]
    public static partial void SchedulerRegistered(ILogger logger, string JobId, string Cron);
}
