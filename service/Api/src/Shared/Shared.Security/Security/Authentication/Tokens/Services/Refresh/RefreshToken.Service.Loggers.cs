namespace Shared.Security.Authentication.Tokens.Services.Refresh;

public partial class RefreshTokenService
{
    /// <summary>Structured log event definitions for refresh token lifecycle operations.</summary>
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 207,
            Level = LogLevel.Information,
            Message = "Refresh token generated for user {UserId}, expires {ExpiresAt}, rotation: {RotationEnabled}")]
        public static partial void LogTokenGenerated(ILogger logger, Guid userId, DateTimeOffset expiresAt, bool rotationEnabled);

        [LoggerMessage(
            EventId = 208,
            Level = LogLevel.Error,
            Message = "Failed to generate refresh token for user {UserId}")]
        public static partial void LogTokenGenerationFailed(ILogger logger, Guid userId, Exception ex);

        [LoggerMessage(
            EventId = 209,
            Level = LogLevel.Debug,
            Message = "Sliding expiration applied for token {TokenId}, new expiry: {ExpiresAt}")]
        public static partial void LogSlidingExpirationApplied(ILogger logger, Guid tokenId, DateTimeOffset expiresAt);

        [LoggerMessage(
            EventId = 210,
            Level = LogLevel.Information,
            Message = "Refresh token {TokenId} revoked for user {UserId}. Reason: {Reason}")]
        public static partial void LogTokenRevoked(ILogger logger, Guid tokenId, Guid userId, string? reason);

        [LoggerMessage(
            EventId = 211,
            Level = LogLevel.Warning,
            Message = "All {Count} refresh tokens revoked for user {UserId}. Reason: {Reason}")]
        public static partial void LogAllTokensRevoked(ILogger logger, int count, Guid userId, string reason);

        [LoggerMessage(
            EventId = 212,
            Level = LogLevel.Information,
            Message = "Refresh token rotated for user {UserId}. Old: {OldId}, New: {NewId}")]
        public static partial void LogTokenRotated(ILogger logger, Guid userId, Guid oldId, Guid newId);

        [LoggerMessage(
            EventId = 213,
            Level = LogLevel.Error,
            Message = "Failed to rotate refresh token for user {UserId}")]
        public static partial void LogTokenRotationFailed(ILogger logger, Guid userId, Exception ex);
    }
}
