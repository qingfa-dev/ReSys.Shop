namespace Shared.Security.Authentication.External.Providers.Facebook;

public sealed partial class FacebookExternalProvider
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 3004,
            Level = LogLevel.Warning,
            Message = "Facebook access token validation failed")]
        public static partial void TokenValidationError(ILogger logger, Exception ex);
    }
}
