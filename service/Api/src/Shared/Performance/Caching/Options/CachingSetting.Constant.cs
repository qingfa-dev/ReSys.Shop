using ReSys.Shop.ServiceDefaults.Constants;

namespace Shared.Performance.Caching.Options;

public static class CachingSettingConstant
{
    // Connection string keys
    public const string Aspire = Infrastructures.Cache.RedisResource;      // Aspire-managed
    public const string Default = "DefaultCaching";  // Standalone
}