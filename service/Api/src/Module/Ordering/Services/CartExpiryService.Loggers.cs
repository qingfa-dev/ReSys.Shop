namespace Module.Ordering.Services;

public sealed partial class CartExpiryService
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 5004,
            Level = LogLevel.Information,
            Message = "[CartExpiry.ServiceStarted]: Cart-expiry service started")]
        public static partial void Started(ILogger logger);

        [LoggerMessage(
            EventId = 5005,
            Level = LogLevel.Error,
            Message = "[CartExpiry.SweepError]: Error during cart expiry sweep")]
        public static partial void SweepError(ILogger logger, Exception ex);
    }
}
