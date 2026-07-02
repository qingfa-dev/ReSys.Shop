using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

using Shared.Performance.Caching.Options;

namespace Shared.Performance.Caching.Wrappers;

/// <summary>
/// Implementation of ICacheService that leverages .NET 10 HybridCache.
/// Provides L1 (Memory) and L2 (Distributed) multi-tier caching.
/// </summary>
/// <param name="hybridCache">The underlying HybridCache instance.</param>
/// <param name="cachingOptions">The caching configuration options.</param>
/// <param name="logger">The logger for caching operations.</param>
public sealed partial class CacheService(
    HybridCache hybridCache,
    IOptions<CachingSetting> cachingOptions,
    ILogger<CacheService> logger) : ICacheService
{
    /// <inheritdoc />
    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        CachingEntryOption? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        // Guard: Bypass cache if disabled and execute factory directly
        if (!cachingOptions.Value.Enabled)
        {
            Loggers.CacheBypassed(logger, key);
            return await factory(cancellationToken);
        }

        // Await: Retrieve from L1/L2 or generate and store via HybridCache
        var hceOptions = options.ToHybridCacheEntryOptions();
        T result = await hybridCache.GetOrCreateAsync(
            key,
            factory,
            hceOptions,
            tags,
            cancellationToken);

        Loggers.CacheHit(logger, key);
        return result;
    }

    /// <inheritdoc />
    public async ValueTask SetAsync<T>(
        string key,
        T value,
        CachingEntryOption? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        // Guard: Skip operation if caching is disabled
        if (!cachingOptions.Value.Enabled)
        {
            Loggers.CacheBypassed(logger, key);
            return;
        }

        // Call: Persist value in the cache hierarchy
        var hceOptions = options.ToHybridCacheEntryOptions();
        await hybridCache.SetAsync(
            key,
            value,
            hceOptions,
            tags,
            cancellationToken);

        Loggers.CacheSet(logger, key);
    }

    /// <inheritdoc />
    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Guard: Skip operation if caching is disabled
        if (!cachingOptions.Value.Enabled)
        {
            Loggers.CacheBypassed(logger, key);
            return;
        }

        // Call: Invalidate specific entry across all tiers
        await hybridCache.RemoveAsync(key, cancellationToken);

        Loggers.CacheRemoved(logger, key);
    }

    /// <inheritdoc />
    public async ValueTask RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        // Guard: Skip operation if caching is disabled
        if (!cachingOptions.Value.Enabled)
        {
            return;
        }

        // Call: Bulk invalidate entries associated with specified tags
        IEnumerable<string> enumerable = tags as string[] ?? [.. tags];
        await hybridCache.RemoveByTagAsync(enumerable, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            var tagsString = string.Join(", ", enumerable);
            Loggers.CacheRemovedByTag(logger, tagsString);
        }
    }

    /// <inheritdoc />
    public async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // Guard: Skip operation if caching is disabled
        if (!cachingOptions.Value.Enabled)
        {
            return;
        }

        // Call: Invalidate all entries associated with a single tag
        await hybridCache.RemoveByTagAsync(tag, cancellationToken);

        Loggers.CacheRemovedByTag(logger, tag);
    }
}
