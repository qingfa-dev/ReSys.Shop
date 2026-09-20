namespace Shared.Performance.Caching.Options.Hybrid;

public sealed class HybridCacheSetting
{
    public bool Enabled { get; set; } = HybridCacheSettingConstant.Defaults.Enabled;

    public int DefaultExpirationMinutes { get; set; } = HybridCacheSettingConstant.Defaults.DefaultExpirationMinutes;

    public long MaximumPayloadBytes { get; set; } = HybridCacheSettingConstant.Defaults.MaximumPayloadBytes;

    public int MaximumKeyLength { get; set; } = HybridCacheSettingConstant.Defaults.MaximumKeyLength;

    // Helper property for TimeSpan
    public TimeSpan DefaultExpiration => TimeSpan.FromMinutes(DefaultExpirationMinutes);
}