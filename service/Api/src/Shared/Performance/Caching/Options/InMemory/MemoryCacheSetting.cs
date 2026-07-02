namespace Shared.Performance.Caching.Options.InMemory;

public sealed class MemoryCacheSetting
{
    public bool Enabled { get; set; } = MemoryCacheConstants.Defaults.Enabled;

    public int DefaultExpirationMinutes { get; set; } = MemoryCacheConstants.Defaults.DefaultExpirationMinutes;

    /// <summary>
    /// Percentage of cache to compact when memory pressure is detected.
    /// </summary>
    public int CompactionPercentage { get; set; } = MemoryCacheConstants.Defaults.CompactionPercentage;

    // Optional: add a size limit in bytes (e.g., for Microsoft.Extensions.Caching.Memory)
    public long? SizeLimitBytes { get; set; } = MemoryCacheConstants.Defaults.SizeLimitBytes;

    public TimeSpan DefaultExpiration => TimeSpan.FromMinutes(DefaultExpirationMinutes);
}