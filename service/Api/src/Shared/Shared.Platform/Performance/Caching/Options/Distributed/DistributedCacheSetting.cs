namespace Shared.Performance.Caching.Options.Distributed;

/// <summary>
/// Options for distributed (L2) cache configuration.
/// </summary>
public sealed class DistributedCacheSetting
{
    /// <summary>
    /// Enable distributed cache.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Distributed cache type: "redis" or "sqlserver".
    /// </summary>
    public string Type { get; set; } = "redis";

    /// <summary>
    /// Default distributed cache entry expiration in minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Indicates whether a connection string is required for this distributed cache.
    /// Returns true when Enabled is true.
    /// </summary>
    public bool Required => Enabled;
}
