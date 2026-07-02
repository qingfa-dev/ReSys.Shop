using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

/// <summary>
/// Implementation of <see cref="ITokenTheftDetector"/> using hybrid cache with database fallback.
/// </summary>
public sealed partial class TokenTheftDetector(
    IRefreshTokenStore refreshTokenStore,
    HybridCache hybridCache,
    IOptions<TokenSecurityOptions> options,
    ILogger<TokenTheftDetector> logger) : ITokenTheftDetector
{
    private readonly TokenSecurityOptions _options = options.Value;
    private const string CachePrefix = "token_theft:";

    /// <inheritdoc/>
    public async Task<Result<bool>> IsTokenReusedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        // Guard: Skip check if theft detection is globally disabled
        if (!_options.ReuseDetectionEnabled)
        {
            return Result<bool>.Ok(false);
        }

        // Compute: Generate deterministic hash of the token for secure lookup
        string tokenHash = ComputeTokenHash(token);
        string cacheKey = $"{CachePrefix}{userId}:{tokenHash}";

        try
        {
            // Cache: Probe for previous usage of this specific token
            string? existing = await hybridCache.GetOrCreateAsync(
                cacheKey, _ => ValueTask.FromResult((string?)null),
                tags: ["token-theft", $"user:{userId}"],
                cancellationToken: ct);

            if (existing != null)
            {
                // Log: High-severity security alert for detected token reuse
                Loggers.LogTokenReuseDetected(logger, userId);

                // Call: Execute emergency revocation of all user sessions
                await RevokeAllUserTokensAsync(userId, "reuse_detected", ct);
                return Result<bool>.Ok(true);
            }

            return Result<bool>.Ok(false);
        }
        catch (Exception ex)
        {
            // Fallback: Attempt database-level check if the cache is unavailable
            Loggers.LogCacheCheckFailed(logger, ex);

            try
            {
                // Call: Look up the token by hash in the primary store
                RefreshToken? tokenEntity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

                // Check: Token exists and was previously replaced — indicates replay attack
                if (tokenEntity != null && tokenEntity.RevocationReason == RefreshTokenRevocationReason.Replaced)
                {
                    // Log: Security alert based on database evidence
                    Loggers.LogTokenReuseDetectedInDb(logger, userId);

                    // Call: Emergency revocation of all sessions
                    await RevokeAllUserTokensAsync(userId, "reuse_detected", ct);
                    return Result<bool>.Ok(true);
                }

                return Result<bool>.Ok(false);
            }
            catch (Exception dbEx)
            {
                // Log: Infrastructure failure — unable to verify reuse
                Loggers.LogDbCheckFailed(logger, dbEx);
                return RefreshTokenResult.Failure.TokenTheftDetectorFailure;
            }
        }
    }

    /// <inheritdoc/>
    public async Task MarkTokenAsUsedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        // Guard: Skip if detection is disabled
        if (!_options.ReuseDetectionEnabled)
        {
            return;
        }

        // Compute: Generate hash for cache key
        string tokenHash = ComputeTokenHash(token);
        string cacheKey = $"{CachePrefix}{userId}:{tokenHash}";

        try
        {
            // Cache: Register the token as used in the current session
            await hybridCache.SetAsync(
                cacheKey,
                "used",
                tags: ["token-theft", $"user:{userId}"],
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            // Fallback: Attempt to persist usage marker to database if cache fails
            Loggers.LogMarkTokenCacheFailed(logger, ex);

            try
            {
                // Call: Fetch token record from primary store by computed hash
                RefreshToken? tokenEntity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);
                if (tokenEntity != null)
                {
                    // Update: Touch LastUsedAtUtc to record usage on the persistent record
                    tokenEntity.LastUsedAtUtc = DateTimeOffset.UtcNow;
                    await refreshTokenStore.UpdateAsync(tokenEntity, ct);
                }
            }
            catch (Exception dbEx)
            {
                // Log: Persistent storage error — non-fatal
                Loggers.LogMarkTokenDbFailed(logger, dbEx);
            }
        }
    }

    /// <inheritdoc/>
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        // Call: Fetch all currently active tokens for the user
        List<RefreshToken> activeTokens = await refreshTokenStore.GetActiveByUserIdAsync(userId, ct);

        // Update: Mark each active token as revoked
        DateTime now = DateTime.UtcNow;
        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAtUtc = now;
            token.RevocationReason = MapToRevocationReason(reason);
            await refreshTokenStore.UpdateAsync(token, ct);
        }

        if (activeTokens.Count != 0)
        {
            // Log: Audit record for mass session invalidation
            Loggers.LogAllTokensRevoked(logger, activeTokens.Count, userId, reason);
        }

        // Call: Purge the user's theft-detection entries from cache
        await CleanupCacheForUserAsync(userId, ct);
    }

    // Compute: SHA256 hash for consistent cache and store keying
    private static string ComputeTokenHash(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // Map: String reason to domain enumeration
    private static RefreshTokenRevocationReason MapToRevocationReason(string reason)
    {
        return reason.ToLowerInvariant() switch
        {
            "reuse_detected" => RefreshTokenRevocationReason.ReuseDetected,
            "user_logout" => RefreshTokenRevocationReason.UserLogout,
            "user_logout_all" => RefreshTokenRevocationReason.UserLogoutAll,
            "replaced" => RefreshTokenRevocationReason.Replaced,
            _ => RefreshTokenRevocationReason.ReuseDetected
        };
    }

    // Cleanup: Remove cached theft-detection entries for a user
    private async Task CleanupCacheForUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            await hybridCache.RemoveByTagAsync($"user:{userId}", ct);
        }
        catch (Exception ex)
        {
            Loggers.LogCacheCleanupFailed(logger, userId, ex);
        }
    }
}

/// <summary>
/// A "Null Object" implementation of <see cref="ITokenTheftDetector"/> that always returns success
/// and detects no reuse. This is used when token reuse detection is disabled.
/// </summary>
public class NoOpTokenTheftDetector : ITokenTheftDetector
{
    /// <inheritdoc/>
    public Task<Result<bool>> IsTokenReusedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        // When disabled, we never report reuse (false means not detected).
        return Task.FromResult(Result<bool>.Ok(false));
    }

    /// <inheritdoc/>
    public Task MarkTokenAsUsedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        // No-op implementation does nothing.
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        // No-op implementation does nothing.
        return Task.CompletedTask;
    }
}
