using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

using Shared.Security.Authentication.Tokens.Options;
using Shared.Security.Authentication.Tokens.Services.Refresh.Store;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

/// <summary>Detects refresh token reuse (theft) using hybrid cache with database fallback — mitigates TMT-TOK-004.</summary>
// Invariant: Token hash is deterministic SHA256; cache key includes userId to prevent cross-user collision; cache failure degrades to database check.
// Context: Token replay attacks occur when an attacker steals a refresh token and uses it after legitimate rotation. This detector flags the legitimate owner's next use as theft.
public sealed partial class TokenTheftDetector(
    IRefreshTokenStore refreshTokenStore,
    HybridCache hybridCache,
    IOptions<TokenSecurityOptions> options,
    ILogger<TokenTheftDetector> logger) : ITokenTheftDetector
{
    private readonly TokenSecurityOptions _options = options.Value;
    private const string CachePrefix = "token_theft:";

    /// <summary>Checks if a token has been used before (indicating theft), with cache-first and database-fallback.</summary>
    // Contract: pre=token!=null && userId!=Guid.Empty, post=return.IsSuccess, throws=Exception on combined cache+db failure
    public async Task<Result<bool>> IsTokenReusedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        // Guard: skip check when detection is globally disabled to avoid unnecessary work
        if (!_options.ReuseDetectionEnabled)
        {
            return Result<bool>.Ok(false);
        }

        // Compute: deterministic SHA256 hash for secure cache and store keying — raw token never stored
        string tokenHash = ComputeTokenHash(token);
        string cacheKey = $"{CachePrefix}{userId}:{tokenHash}";

        try
        {
            // Cache: probe HybridCache for previous usage of this token (module boundary: Detector → Cache)
            string? existing = await hybridCache.GetOrCreateAsync(
                cacheKey, _ => ValueTask.FromResult((string?)null),
                tags: ["token-theft", $"user:{userId}"],
                cancellationToken: ct);

            if (existing != null)
            {
                // Log: high-severity security alert for detected token reuse
                Loggers.LogTokenReuseDetected(logger, userId);

                // Call: emergency revocation of all user sessions as containment measure
                await RevokeAllUserTokensAsync(userId, "reuse_detected", ct);
                return Result<bool>.Ok(true);
            }

            return Result<bool>.Ok(false);
        }
        catch (Exception ex)
        {
            // Fallback: attempt database-level check when cache is unavailable — degrades, does not fail open
            Loggers.LogCacheCheckFailed(logger, ex);

            try
            {
                // Call: lookup token in primary store (module boundary: Detector → Store)
                RefreshToken? tokenEntity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);

                // Validate: previously-replaced token in DB confirms replay attack
                if (tokenEntity != null && tokenEntity.RevocationReason == RefreshTokenRevocationReason.Replaced)
                {
                    // Log: security alert based on database evidence
                    Loggers.LogTokenReuseDetectedInDb(logger, userId);

                    // Call: emergency revocation of all sessions
                    await RevokeAllUserTokensAsync(userId, "reuse_detected", ct);
                    return Result<bool>.Ok(true);
                }

                return Result<bool>.Ok(false);
            }
            catch (Exception dbEx)
            {
                // Catch: both cache and DB unavailable — failure is safer than false negative
                Loggers.LogDbCheckFailed(logger, dbEx);
                return Result<bool>.Unexpected(
                    exception: dbEx,
                    errors: [RefreshTokenResult.Failure.TokenTheftDetectorFailure]);
            }
        }
    }

    /// <summary>Records a token as used to detect future replays.</summary>
    // Contract: pre=token!=null && userId!=Guid.Empty, post=token marked used, throws=non-fatal (best-effort)
    public async Task MarkTokenAsUsedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        // Guard: skip when detection is disabled
        if (!_options.ReuseDetectionEnabled)
        {
            return;
        }

        // Compute: deterministic hash for cache key
        string tokenHash = ComputeTokenHash(token);
        string cacheKey = $"{CachePrefix}{userId}:{tokenHash}";

        try
        {
            // Cache: register token as used via HybridCache (module boundary: Detector → Cache)
            await hybridCache.SetAsync(
                cacheKey,
                "used",
                tags: ["token-theft", $"user:{userId}"],
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            // Fallback: touch LastUsedAtUtc in database when cache is unavailable
            Loggers.LogMarkTokenCacheFailed(logger, ex);

            try
            {
                // Call: lookup token in store and update timestamp (module boundary: Detector → Store)
                RefreshToken? tokenEntity = await refreshTokenStore.GetByTokenHashAsync(tokenHash, ct);
                if (tokenEntity != null)
                {
                    // Update: set LastUsedAtUtc to now — database as fallback usage marker
                    tokenEntity.LastUsedAtUtc = DateTimeOffset.UtcNow;
                    await refreshTokenStore.UpdateAsync(tokenEntity, ct);
                }
            }
            catch (Exception dbEx)
            {
                // Catch: both cache and DB unavailable — non-fatal, theft detection may miss this replay
                Loggers.LogMarkTokenDbFailed(logger, dbEx);
            }
        }
    }

    /// <summary>Revokes all active tokens for a user and purges theft-detection cache entries — used during theft response.</summary>
    // Contract: pre=userId!=Guid.Empty, post=all active tokens revoked, throws=non-fatal (best-effort)
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        // Call: fetch all currently active tokens (module boundary: Detector → Store)
        List<RefreshToken> activeTokens = await refreshTokenStore.GetActiveByUserIdAsync(userId, ct);

        // Update: revoke each active token to invalidate all sessions
        DateTime now = DateTime.UtcNow;
        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAtUtc = now;
            token.RevocationReason = MapToRevocationReason(reason);
            await refreshTokenStore.UpdateAsync(token, ct);
        }

        if (activeTokens.Count != 0)
        {
            // Log: audit record for mass session invalidation
            Loggers.LogAllTokensRevoked(logger, activeTokens.Count, userId, reason);
        }

        // Call: purge theft-detection cache entries for user (module boundary: Detector → Cache)
        await CleanupCacheForUserAsync(userId, ct);
    }

    // Compute: SHA256 hash for deterministic cache and store keying — raw token never stored or logged
    private static string ComputeTokenHash(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // Transform: string reason string to domain enumeration for consistent audit trail
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

    // Cache: purge theft-detection entries for user to clean up after mass revocation
    private async Task CleanupCacheForUserAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            await hybridCache.RemoveByTagAsync($"user:{userId}", ct);
        }
        catch (Exception ex)
        {
            // Catch: cache cleanup failure is non-fatal — stale entries expire via TTL
            Loggers.LogCacheCleanupFailed(logger, userId, ex);
        }
    }
}

/// <summary>Null-object implementation that returns "not reused" for all checks — used when theft detection is disabled.</summary>
// Invariant: Always returns false for IsTokenReusedAsync; all mutating methods are no-ops.
public class NoOpTokenTheftDetector : ITokenTheftDetector
{
    /// <summary>Always reports no reuse — theft detection is disabled.</summary>
    // Contract: post=return.IsSuccess && return.Value==false
    public Task<Result<bool>> IsTokenReusedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(Result<bool>.Ok(false));
    }

    /// <summary>No-op — theft detection is disabled.</summary>
    // Contract: post=no side effects
    public Task MarkTokenAsUsedAsync(string token, Guid userId, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <summary>No-op — theft detection is disabled.</summary>
    // Contract: post=no side effects
    public Task RevokeAllUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
