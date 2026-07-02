using Shared.Performance.Caching.Wrappers;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

/// <summary>
/// Implementation of <see cref="ITokenBlacklistService"/> using hybrid cache.
/// </summary>
public sealed partial class TokenBlacklistService(
    ICacheService cacheService,
    ILogger<TokenBlacklistService> logger) : ITokenBlacklistService
{
    private const string BlacklistPrefix = "blacklist:";

    /// <inheritdoc/>
    public async Task<Result> IsBlacklistedAsync(string jti, CancellationToken ct = default)
    {
        // Guard: Return false immediately if no JTI is provided
        if (string.IsNullOrEmpty(jti))
            return TokenBlacklistResult.Failure.NotBlacklisted;

        // Generate: Construct the unique cache key for this token
        var cacheKey = $"{BlacklistPrefix}{jti}";

        try
        {
            // Cache: Probe the HybridCache for existence of the blacklisted JTI
            var result = await cacheService.GetOrCreateAsync(
                cacheKey,
                async _ => (string?)null,
                tags: ["blacklist"],
                cancellationToken: ct);

            // Transform: Map cache presence to boolean result
            return result != null
                ? TokenBlacklistResult.Success.Blacklisted
                : TokenBlacklistResult.Failure.NotBlacklisted;
        }
        catch (Exception ex)
        {
            // Log: Detailed warning for cache access failures
            Loggers.LogCheckBlacklistFailed(logger, ex);
            return TokenBlacklistResult.Failure.BlacklistCheckFailed;
        }
    }

    /// <inheritdoc/>
    public async Task<Result> BlacklistTokenAsync(string jti, DateTime expiry, CancellationToken ct = default)
    {
        // Guard: Skip operation if JTI is missing
        if (string.IsNullOrEmpty(jti))
            return Result.Ok();

        // Generate: Construct the unique cache key for this token
        var cacheKey = $"{BlacklistPrefix}{jti}";

        try
        {
            // Compute: Determine remaining time-to-live based on token expiry
            TimeSpan ttl = expiry - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                // Log: Ignore already expired tokens
                Loggers.LogTokenAlreadyExpired(logger);
                return TokenBlacklistResult.Success.Blacklisted;
            }

            // Initialize: Configure cache duration with a small safety buffer
            var entryOptions = new CachingEntryOption
            {
                Expiration = ttl.Add(TimeSpan.FromMinutes(5))
            };

            // Cache: Persist the JTI in the blacklist with expiration
            await cacheService.SetAsync(
                cacheKey,
                "blacklisted",
                entryOptions,
                tags: ["blacklist"],
                cancellationToken: ct);

            // Log: Record security event
            Loggers.LogTokenBlacklisted(logger, jti, expiry);
            return TokenBlacklistResult.Success.Blacklisted;
        }
        catch (Exception ex)
        {
            // Log: Detailed error for blacklist persistence failure
            Loggers.LogBlacklistTokenFailed(logger, jti, ex);
            return TokenBlacklistResult.Failure.BlacklistFailed;
        }
    }

    /// <inheritdoc/>
    public Task<Result> CleanupExpiredAsync(CancellationToken ct = default)
    {
        return Task.FromResult(Result.Ok());
    }
}
