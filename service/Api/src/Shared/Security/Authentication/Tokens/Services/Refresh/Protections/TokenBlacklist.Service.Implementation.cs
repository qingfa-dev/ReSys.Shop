using Shared.Performance.Caching.Wrappers;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

/// <summary>Implements a hybrid-cache-backed blacklist for immediate token invalidation.</summary>
// Invariant: Blacklist entries expire with the token's TTL plus safety buffer; expired tokens are never blacklisted.
// Context: Blacklist provides fast O(1) revocation check for JWT JTIs without database round-trip.
public sealed partial class TokenBlacklistService(
    ICacheService cacheService,
    ILogger<TokenBlacklistService> logger) : ITokenBlacklistService
{
    private const string BlacklistPrefix = "blacklist:";

    /// <summary>Checks whether a given JTI is in the blacklist.</summary>
    // Contract: pre=jti!=null, post=return.IsSuccess, throws=Exception on cache failure
    public async Task<Result> IsBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        // Guard: null or empty JTI cannot be blacklisted — return false immediately
        if (string.IsNullOrEmpty(jti))
            return TokenBlacklistResult.Failure.NotBlacklisted;

        // Compute: deterministic cache key from JTI for O(1) lookup
        var cacheKey = $"{BlacklistPrefix}{jti}";

        try
        {
            // Cache: probe HybridCache for blacklisted JTI existence (module boundary: Service → Cache)
            var result = await cacheService.GetOrCreateAsync(
                cacheKey,
                async _ => (string?)null,
                tags: ["blacklist"],
                cancellationToken: ct);

            // Transform: non-null result means JTI was previously blacklisted
            return result != null
                ? TokenBlacklistResult.Success.Blacklisted
                : TokenBlacklistResult.Failure.NotBlacklisted;
        }
        catch (Exception ex)
        {
            // Catch: cache failure must not crash the request — return check-failed result
            Loggers.LogCheckBlacklistFailed(logger, ex);
            return TokenBlacklistResult.Failure.BlacklistCheckFailed;
        }
    }

    /// <summary>Adds a JTI to the blacklist with TTL matching the original token expiry plus a safety buffer.</summary>
    // Contract: pre=jti!=null && expiry>UtcNow, post=return.IsSuccess, throws=Exception on cache persistence failure
    public async Task<Result> BlacklistTokenAsync(string jti, DateTime expiry, CancellationToken ct = default)
    {
        // Guard: skip blacklisting for empty JTI — no-op is safe
        if (string.IsNullOrEmpty(jti))
            return Result.Ok();

        // Compute: deterministic cache key from JTI
        var cacheKey = $"{BlacklistPrefix}{jti}";

        try
        {
            // Compute: remaining TTL to set cache expiry aligned with token lifetime
            TimeSpan ttl = expiry - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                // Guard: already-expired token needs no blacklisting
                Loggers.LogTokenAlreadyExpired(logger);
                return TokenBlacklistResult.Success.Blacklisted;
            }

            // Compute: add 5-minute safety buffer to handle clock skew between services
            var entryOptions = new CachingEntryOption
            {
                Expiration = ttl.Add(TimeSpan.FromMinutes(5))
            };

            // Cache: persist JTI in blacklist with expiration (boundary: Service → Cache)
            await cacheService.SetAsync(
                cacheKey,
                "blacklisted",
                entryOptions,
                tags: ["blacklist"],
                cancellationToken: ct);

            // Log: record security event for audit
            Loggers.LogTokenBlacklisted(logger, jti, expiry);
            return TokenBlacklistResult.Success.Blacklisted;
        }
        catch (Exception ex)
        {
            // Catch: blacklist persistence failure must not block caller — log and return failure
            Loggers.LogBlacklistTokenFailed(logger, jti, ex);
            return TokenBlacklistResult.Failure.BlacklistFailed;
        }
    }

    /// <summary>No-op cleanup — HybridCache evicts entries automatically via TTL.</summary>
    // Contract: post=return.IsSuccess
    public Task<Result> CleanupExpiredAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Result.Ok());
    }
}
