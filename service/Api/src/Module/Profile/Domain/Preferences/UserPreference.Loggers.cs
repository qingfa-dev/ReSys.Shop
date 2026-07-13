namespace Module.Profile.Domain.Preferences;

public static partial class UserPreferenceLoggers
{
    public static partial class Management
    {
        [LoggerMessage(
            EventId = 1,
            Level = LogLevel.Debug,
            Message = "[UserPreferences.Created]: User {UserId} preferences created")]
        public static partial void Created(ILogger logger, Guid UserId);

        [LoggerMessage(
            EventId = 2,
            Level = LogLevel.Debug,
            Message = "[UserPreferences.Updated]: User {UserId} preferences updated")]
        public static partial void Updated(ILogger logger, Guid UserId);
    }
}