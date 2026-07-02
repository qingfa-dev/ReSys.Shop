using Microsoft.Extensions.Caching.Hybrid;

namespace Shared.Performance.Caching.Wrappers;

internal static class CachingEntryOptionConverter
{
    public static HybridCacheEntryOptions? ToHybridCacheEntryOptions(this CachingEntryOption? option)
    {
        if (option is null)
            return null;

        return new HybridCacheEntryOptions
        {
            Expiration = option.Expiration,
            LocalCacheExpiration = option.LocalCacheExpiration,
            // Flags = option.Flags // reserved for future use
        };
    }
}
