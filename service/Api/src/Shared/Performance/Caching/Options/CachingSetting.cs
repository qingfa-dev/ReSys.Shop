using Shared.Performance.Caching.Options.Distributed;
using Shared.Performance.Caching.Options.Hybrid;
using Shared.Performance.Caching.Options.InMemory;

namespace Shared.Performance.Caching.Options;

/// <summary>
/// Options for caching configuration.
/// Supports HybridCache, IMemoryCache, and IDistributedCache.
/// </summary>
public sealed class CachingSetting
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Caching";

    /// <summary>
    /// Enable/disable caching globally.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Memory cache options (L1 cache).
    /// </summary>
    public MemoryCacheSetting Memory { get; set; } = new();

    /// <summary>
    /// Distributed cache options (L2 cache).
    /// </summary>
    public DistributedCacheSetting Distributed { get; set; } = new();

    /// <summary>
    /// Hybrid cache options (L1 + L2 combined).
    /// </summary>
    public HybridCacheSetting Hybrid { get; set; } = new();
}
