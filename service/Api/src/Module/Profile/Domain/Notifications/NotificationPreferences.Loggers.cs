namespace Module.Profile.Domain.Notifications;

public static partial class NotificationPreferencesLoggers
{
    public static partial class Management
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "[NotificationPreferences.Created]: User {UserId} notification preferences created")]
        public static partial void Created(ILogger logger, Guid UserId);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "[NotificationPreferences.Updated]: User {UserId} notification preferences updated")]
        public static partial void Updated(ILogger logger, Guid UserId);
    }
}
