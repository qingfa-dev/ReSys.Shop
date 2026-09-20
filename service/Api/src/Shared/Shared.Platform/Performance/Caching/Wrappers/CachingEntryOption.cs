// using Microsoft.Extensions.Caching.Hybrid; // disabled: Flags property commented out below

namespace Shared.Performance.Caching.Wrappers;

public sealed class CachingEntryOption
{
    public TimeSpan? Expiration { get; set; }
    public TimeSpan? LocalCacheExpiration { get; set; }
    // public HybridCacheEntryFlags? Flags { get; set; } // reserved for future use
}
