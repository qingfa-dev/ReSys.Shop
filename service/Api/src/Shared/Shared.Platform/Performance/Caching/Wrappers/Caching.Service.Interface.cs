namespace Shared.Performance.Caching.Wrappers;

/// <summary>
/// Defines a high-level orchestration service for caching operations.
/// Wraps HybridCache to provide a consistent and simplified API.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache or creates it using the provided factory if it doesn't exist.
    /// </summary>
    /// <typeparam name="T">The type of the item being cached.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="factory">The factory function to generate the value if not in cache.</param>
    /// <param name="options">Optional entry-specific configuration.</param>
    /// <param name="tags">Optional tags for bulk invalidation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        CachingEntryOption? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Directly sets a value in the cache.
    /// </summary>
    /// <typeparam name="T">The type of the item being cached.</typeparam>
    /// <param name="key">The unique cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="options">Optional entry-specific configuration.</param>
    /// <param name="tags">Optional tags for bulk invalidation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask SetAsync<T>(
        string key,
        T value,
        CachingEntryOption? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific item from the cache by its key.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all items associated with the specified tags.
    /// </summary>
    /// <param name="tags">The tags to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask RemoveByTagAsync(IEnumerable<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all items associated with a single specified tag.
    /// </summary>
    /// <param name="tag">The tag to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
