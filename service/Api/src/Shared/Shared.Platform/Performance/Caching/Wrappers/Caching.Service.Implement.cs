using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Performance.Caching.Options;

namespace Shared.Performance.Caching.Wrappers;

/// <summary>Wraps .NET 10 HybridCache providing L1 (memory) and L2 (distributed) multi-tier caching with configurable bypass and tag-based invalidation.</summary>
// Invariant: All operations check Enabled flag first; bypass skips cache entirely and delegates to factory.
// Boundary: Cache → HybridCache — delegates to .NET 10 HybridCache infrastructure; never accesses Redis/memory directly.
public sealed partial class CacheService(
    IServiceProvider serviceProvider,
    IOptions<CachingSetting> cachingOptions,
    ILogger<CacheService> logger) : ICacheService
{
    private HybridCache? _hybridCache;
    private HybridCache? HybridCache => _hybridCache ??= serviceProvider.GetService<HybridCache>();

    /// <summary>Retrieves a cached value by key or creates and stores it via the factory function.</summary>
    // Contract: pre=key!=null, post=return!=null, throws=Exception on HybridCache failure
    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        CachingEntryOption? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        // Guard: bypass cache entirely when disabled and execute factory directly — avoids cache overhead
        if (!cachingOptions.Value.Enabled || HybridCache is null)
        {
            Loggers.CacheBypassed(logger, key);
            return await factory(cancellationToken);
        }

        // Call: retrieve from L1/L2 or generate via HybridCache (module boundary: Cache → HybridCache)
        var hceOptions = options.ToHybridCacheEntryOptions();
        T result = await HybridCache.GetOrCreateAsync(
            key,
            factory,
            hceOptions,
            tags,
            cancellationToken);

        Loggers.CacheHit(logger, key);
        return result;
    }

    /// <summary>Stores a value in the cache with optional TTL and invalidation tags.</summary>
    // Contract: pre=key!=null && value!=null, post=value cached, throws=Exception on HybridCache failure
    public async ValueTask SetAsync<T>(
        string key,
        T value,
        CachingEntryOption? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        // Guard: skip when caching is disabled or HybridCache unavailable
        if (!cachingOptions.Value.Enabled || HybridCache is null)
        {
            Loggers.CacheBypassed(logger, key);
            return;
        }

        // Call: persist value in the cache hierarchy (module boundary: Cache → HybridCache)
        var hceOptions = options.ToHybridCacheEntryOptions();
        await HybridCache.SetAsync(
            key,
            value,
            hceOptions,
            tags,
            cancellationToken);

        Loggers.CacheSet(logger, key);
    }

    /// <summary>Removes a single cache entry by key.</summary>
    // Contract: pre=key!=null, post=entry evicted, throws=Exception on HybridCache failure
    public async ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Guard: skip when caching is disabled or HybridCache unavailable
        if (!cachingOptions.Value.Enabled || HybridCache is null)
        {
            Loggers.CacheBypassed(logger, key);
            return;
        }

        // Call: invalidate entry across all tiers (module boundary: Cache → HybridCache)
        await HybridCache.RemoveAsync(key, cancellationToken);

        Loggers.CacheRemoved(logger, key);
    }

    /// <summary>Bulk-removes cache entries matching any of the specified tags.</summary>
    // Contract: pre=tags!=null, post=matching entries evicted, throws=Exception on HybridCache failure
    public async ValueTask RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default)
    {
        // Guard: skip when caching is disabled or HybridCache unavailable
        if (!cachingOptions.Value.Enabled || HybridCache is null)
        {
            return;
        }

        // Call: bulk invalidate entries by tags (module boundary: Cache → HybridCache)
        IEnumerable<string> enumerable = tags as string[] ?? [.. tags];
        await HybridCache.RemoveByTagAsync(enumerable, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            var tagsString = string.Join(", ", enumerable);
            Loggers.CacheRemovedByTag(logger, tagsString);
        }
    }

    /// <summary>Removes all cache entries associated with a single tag.</summary>
    // Contract: pre=tag!=null, post=matching entries evicted, throws=Exception on HybridCache failure
    public async ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        // Guard: skip when caching is disabled or HybridCache unavailable
        if (!cachingOptions.Value.Enabled || HybridCache is null)
        {
            return;
        }

        // Call: invalidate entries by tag (module boundary: Cache → HybridCache)
        await HybridCache.RemoveByTagAsync(tag, cancellationToken);

        Loggers.CacheRemovedByTag(logger, tag);
    }
}
