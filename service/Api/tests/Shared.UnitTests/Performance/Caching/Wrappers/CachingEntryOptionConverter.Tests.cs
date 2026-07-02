using Shared.Performance.Caching.Wrappers;

namespace Shared.UnitTests.Performance.Caching.Wrappers;

[Trait("Category", "Unit")]
[Trait("Feature", "Caching")]
public class CachingEntryOptionConverterTests
{
    [Fact(DisplayName = "When input is null should return null")]
    public void WhenInputNull_ShouldReturnNull()
    {
        CachingEntryOption? input = null;

        var result = input.ToHybridCacheEntryOptions();

        result.Should().BeNull();
    }

    [Fact(DisplayName = "When all properties set should map correctly")]
    public void WhenAllPropertiesSet_ShouldMapCorrectly()
    {
        var expiration = TimeSpan.FromMinutes(30);
        var localExpiration = TimeSpan.FromMinutes(10);
        var input = new CachingEntryOption
        {
            Expiration = expiration,
            LocalCacheExpiration = localExpiration,
            // Flags = Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableLocalCache // reserved
        };

        var result = input.ToHybridCacheEntryOptions();

        result.Should().NotBeNull();
        result!.Expiration.Should().Be(expiration);
        result.LocalCacheExpiration.Should().Be(localExpiration);
        // result.Flags.Should().Be(Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableLocalCache); // reserved
    }

    [Fact(DisplayName = "When only Expiration set should map partial")]
    public void WhenOnlyExpirationSet_ShouldMapPartial()
    {
        var expiration = TimeSpan.FromMinutes(15);
        var input = new CachingEntryOption
        {
            Expiration = expiration
        };

        var result = input.ToHybridCacheEntryOptions();

        result.Should().NotBeNull();
        result!.Expiration.Should().Be(expiration);
        result.LocalCacheExpiration.Should().BeNull();
        // result.Flags.Should().BeNull(); // reserved
    }

    [Fact(DisplayName = "When only LocalCacheExpiration set should map partial")]
    public void WhenOnlyLocalExpirationSet_ShouldMapPartial()
    {
        var localExpiration = TimeSpan.FromMinutes(5);
        var input = new CachingEntryOption
        {
            LocalCacheExpiration = localExpiration
        };

        var result = input.ToHybridCacheEntryOptions();

        result.Should().NotBeNull();
        result!.Expiration.Should().BeNull();
        result.LocalCacheExpiration.Should().Be(localExpiration);
        // result.Flags.Should().BeNull(); // reserved
    }

    // [Fact(DisplayName = "When only Flags set should map partial")]
    // public void WhenOnlyFlagsSet_ShouldMapPartial()
    // {
    //     var input = new CachingEntryOption
    //     {
    //         Flags = Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableLocalCache |
    //                 Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCache
    //     };
    //
    //     var result = input.ToHybridCacheEntryOptions();
    //
    //     result.Should().NotBeNull();
    //     result!.Expiration.Should().BeNull();
    //     result.LocalCacheExpiration.Should().BeNull();
    //     result.Flags.Should().Be(
    //         Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableLocalCache |
    //         Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryFlags.DisableDistributedCache);
    // }
}
