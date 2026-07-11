namespace Shared.Security.Authentication.External.Providers.Google;

public sealed partial class GoogleExternalProvider
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 3005,
            Level = LogLevel.Warning,
            Message = "Google ID token validation failed")]
        public static partial void TokenValidationError(ILogger logger, Exception ex);
    }
}
