using System.Net;

namespace Shared.Security.Authentication.External.Providers.Facebook;

public sealed partial class FacebookTokenValidator
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 3003,
            Level = LogLevel.Warning,
            Message = "Facebook token validation failed: {StatusCode}")]
        public static partial void ValidationFailed(ILogger logger, HttpStatusCode StatusCode);
    }
}
