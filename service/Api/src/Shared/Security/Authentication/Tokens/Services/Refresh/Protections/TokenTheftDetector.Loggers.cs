namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

public sealed partial class TokenTheftDetector
{
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 107,
            Level = LogLevel.Warning,
            Message = "Token reuse detected for user {UserId}. Possible token theft.")]
        public static partial void LogTokenReuseDetected(ILogger logger, Guid userId);

        [LoggerMessage(
            EventId = 108,
            Level = LogLevel.Warning,
            Message = "Failed to check HybridCache for token reuse, falling back to database")]
        public static partial void LogCacheCheckFailed(ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = 109,
            Level = LogLevel.Warning,
            Message = "Token reuse detected in database for user {UserId}. Possible token theft.")]
        public static partial void LogTokenReuseDetectedInDb(ILogger logger, Guid userId);

        [LoggerMessage(
            EventId = 110,
            Level = LogLevel.Error,
            Message = "Failed to check database for token reuse")]
        public static partial void LogDbCheckFailed(ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = 111,
            Level = LogLevel.Warning,
            Message = "Failed to mark token in HybridCache, storing in database instead")]
        public static partial void LogMarkTokenCacheFailed(ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = 112,
            Level = LogLevel.Error,
            Message = "Failed to mark token as used in database")]
        public static partial void LogMarkTokenDbFailed(ILogger logger, Exception ex);

        [LoggerMessage(
            EventId = 113,
            Level = LogLevel.Error,
            Message = "Failed to get active tokens for user {UserId}: {Error}")]
        public static partial void LogGetActiveTokensFailed(ILogger logger, Guid userId, string error);

        [LoggerMessage(
            EventId = 114,
            Level = LogLevel.Warning,
            Message = "All {Count} refresh tokens revoked for user {UserId}. Reason: {Reason}")]
        public static partial void LogAllTokensRevoked(ILogger logger, int count, Guid userId, string reason);

        [LoggerMessage(
            EventId = 115,
            Level = LogLevel.Warning,
            Message = "Failed to cleanup cache for user {UserId}")]
        public static partial void LogCacheCleanupFailed(ILogger logger, Guid userId, Exception ex);
    }
}
