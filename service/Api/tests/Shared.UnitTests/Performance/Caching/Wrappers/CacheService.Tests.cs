using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Shared.Performance.Caching.Options;
using Shared.Performance.Caching.Wrappers;

namespace Shared.UnitTests.Performance.Caching.Wrappers;

[Trait("Category", "Unit")]
[Trait("Feature", "Caching")]
public class CacheServiceTests
{
    private readonly HybridCache _hybridCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<CachingSetting> _enabledOptions;
    private readonly IOptions<CachingSetting> _disabledOptions;

    public CacheServiceTests()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        _serviceProvider = services.BuildServiceProvider();
        _hybridCache = _serviceProvider.GetRequiredService<HybridCache>();

        _enabledOptions = Microsoft.Extensions.Options.Options.Create(new CachingSetting { Enabled = true });
        _disabledOptions = Microsoft.Extensions.Options.Options.Create(new CachingSetting { Enabled = false });
    }

    [Fact(DisplayName = "GetOrCreateAsync with null options should call HybridCache")]
    public async Task GetOrCreateAsync_NullOptions_ShouldCallHybridCache()
    {
        var sut = new CacheService(_serviceProvider, _enabledOptions, NullLogger<CacheService>.Instance);

        var result = await sut.GetOrCreateAsync(
            "test-key-null",
            _ => ValueTask.FromResult("cached-value"),
            options: null);

        result.Should().Be("cached-value");
    }

    [Fact(DisplayName = "GetOrCreateAsync with CachingEntryOption should use converted options")]
    public async Task GetOrCreateAsync_WithOptions_ShouldUseConvertedOptions()
    {
        var sut = new CacheService(_serviceProvider, _enabledOptions, NullLogger<CacheService>.Instance);
        var options = new CachingEntryOption
        {
            Expiration = TimeSpan.FromMinutes(5),
            LocalCacheExpiration = TimeSpan.FromMinutes(2)
        };

        var result = await sut.GetOrCreateAsync(
            "test-key-options",
            _ => ValueTask.FromResult("cached-value-options"),
            options: options);

        result.Should().Be("cached-value-options");
    }

    [Fact(DisplayName = "SetAsync with options should use converted options")]
    public async Task SetAsync_WithOptions_ShouldUseConvertedOptions()
    {
        var sut = new CacheService(_serviceProvider, _enabledOptions, NullLogger<CacheService>.Instance);
        var options = new CachingEntryOption
        {
            Expiration = TimeSpan.FromMinutes(10)
        };

        await sut.SetAsync("test-set-key", "set-value", options: options);

        var retrieved = await sut.GetOrCreateAsync(
            "test-set-key",
            _ => ValueTask.FromResult("not-found"));

        retrieved.Should().Be("set-value");
    }

    [Fact(DisplayName = "When caching disabled should bypass HybridCache")]
    public async Task WhenCachingDisabled_ShouldBypassHybridCache()
    {
        var sut = new CacheService(_serviceProvider, _disabledOptions, NullLogger<CacheService>.Instance);

        var result = await sut.GetOrCreateAsync(
            "disabled-key",
            _ => ValueTask.FromResult("factory-value"),
            options: new CachingEntryOption { Expiration = TimeSpan.FromMinutes(5) });

        result.Should().Be("factory-value");

        var cached = await sut.GetOrCreateAsync(
            "disabled-key",
            _ => ValueTask.FromResult("factory-again"));

        cached.Should().Be("factory-again");
    }

    [Fact(DisplayName = "SetAsync when caching disabled should bypass")]
    public async Task SetAsync_WhenDisabled_ShouldBypass()
    {
        var sut = new CacheService(_serviceProvider, _disabledOptions, NullLogger<CacheService>.Instance);

        await sut.SetAsync("disabled-set", "should-not-be-cached");

        var retrieved = await _hybridCache.GetOrCreateAsync(
            "disabled-set",
            _ => ValueTask.FromResult("default"));

        retrieved.Should().Be("default");
    }
}
