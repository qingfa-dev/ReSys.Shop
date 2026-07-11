namespace Shared.Security.Authentication.External.Providers.Microsoft;

public sealed partial class MicrosoftExternalProvider
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 3002,
            Level = LogLevel.Warning,
            Message = "Microsoft access token validation failed")]
        public static partial void TokenValidationError(ILogger logger, Exception ex);
    }
}
