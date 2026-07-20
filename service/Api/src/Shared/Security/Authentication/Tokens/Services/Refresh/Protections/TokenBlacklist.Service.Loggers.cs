namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

public partial class TokenBlacklistService
{
    /// <summary>Structured log event definitions for token blacklist operations.</summary>
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 203,
            Level = LogLevel.Warning,
            Message = "Failed to check token blacklist in HybridCache")]
        public static partial void LogCheckBlacklistFailed(ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = 204,
            Level = LogLevel.Debug,
            Message = "Token already expired, no need to blacklist")]
        public static partial void LogTokenAlreadyExpired(ILogger logger);

        [LoggerMessage(
            EventId = 205,
            Level = LogLevel.Information,
            Message = "Token {Jti} added to blacklist until {Expiry}")]
        public static partial void LogTokenBlacklisted(ILogger logger, string jti, DateTime expiry);

        [LoggerMessage(
            EventId = 206,
            Level = LogLevel.Error,
            Message = "Failed to blacklist token {Jti}")]
        public static partial void LogBlacklistTokenFailed(ILogger logger, string jti, Exception ex);
    }
}
