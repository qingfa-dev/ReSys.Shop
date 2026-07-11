using System.Net;

namespace Shared.Security.Authentication.External.Providers.Microsoft;

public sealed partial class MicrosoftTokenValidator
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 3001,
            Level = LogLevel.Warning,
            Message = "Microsoft token validation failed: {StatusCode}")]
        public static partial void ValidationFailed(ILogger logger, HttpStatusCode StatusCode);
    }
}
