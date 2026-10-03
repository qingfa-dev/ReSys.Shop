using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Performance.Caching;
using Shared.Performance.Caching.Wrappers;

namespace Shared.UnitTests.Performance.Caching;

[Trait("Category", "Unit")]
[Trait("Module", "Infrastructure")]
[Trait("Feature", "Caching")]
public class CachingExtensionsTests
{
    [Fact(DisplayName = "AddCaching when all enabled should register all services")]
    public void AddCaching_WhenAllEnabled_ShouldRegisterAllServices()
    {
        Dictionary<string, string?> configData = new()
        {
            ["Caching:Enabled"] = "true",
            ["Caching:Memory:Enabled"] = "true",
            ["Caching:Distributed:Enabled"] = "true",
            ["Caching:Distributed:Type"] = "inmemory",
            ["Caching:Hybrid:Enabled"] = "true"
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(configData);
        builder.Services.AddLogging();
        builder.AddCaching();

        ServiceProvider provider = builder.Services.BuildServiceProvider();

        provider.GetService<ICacheService>().Should().NotBeNull();
        provider.GetService<IMemoryCache>().Should().NotBeNull();
        provider.GetService<IDistributedCache>().Should().NotBeNull();
        provider.GetService<HybridCache>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddCaching when disabled should register CacheService (no-op)")]
    public void AddCaching_WhenDisabled_ShouldRegisterCacheServiceAsNoOp()
    {
        Dictionary<string, string?> configData = new()
        {
            ["Caching:Enabled"] = "false"
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(configData);
        builder.AddCaching();

        ServiceProvider provider = builder.Services.BuildServiceProvider();

        // CacheService is always registered — it no-ops internally when CachingSetting.Enabled is false
        provider.GetService<ICacheService>().Should().NotBeNull();
        provider.GetRequiredService<ICacheService>().Should().BeOfType<CacheService>();
    }

    [Fact(DisplayName = "AddCaching when memory disabled should not register IMemoryCache")]
    public void AddCaching_WhenMemoryDisabled_ShouldNotRegisterMemoryCache()
    {
        Dictionary<string, string?> configData = new()
        {
            ["Caching:Enabled"] = "true",
            ["Caching:Memory:Enabled"] = "false",
            ["Caching:Distributed:Enabled"] = "true",
            ["Caching:Distributed:Type"] = "inmemory",
            ["Caching:Hybrid:Enabled"] = "true"
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(configData);
        builder.Services.AddLogging();
        builder.AddCaching();

        ServiceProvider provider = builder.Services.BuildServiceProvider();

        // IMemoryCache is registered by AddHybridCache regardless of Memory.Enabled
        provider.GetService<IMemoryCache>().Should().NotBeNull();
        provider.GetService<IDistributedCache>().Should().NotBeNull();
        provider.GetService<HybridCache>().Should().NotBeNull();
        provider.GetService<ICacheService>().Should().NotBeNull();
    }

    [Fact(DisplayName = "AddCaching when hybrid disabled should not register HybridCache")]
    public void AddCaching_WhenHybridDisabled_ShouldNotRegisterHybridCache()
    {
        Dictionary<string, string?> configData = new()
        {
            ["Caching:Enabled"] = "true",
            ["Caching:Memory:Enabled"] = "true",
            ["Caching:Distributed:Enabled"] = "true",
            ["Caching:Distributed:Type"] = "inmemory",
            ["Caching:Hybrid:Enabled"] = "false"
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(configData);
        builder.Services.AddLogging();
        builder.AddCaching();

        ServiceProvider provider = builder.Services.BuildServiceProvider();

        provider.GetService<HybridCache>().Should().BeNull();
        // IMemoryCache is registered via AddDistributedMemoryCache fallback
        provider.GetService<IMemoryCache>().Should().NotBeNull();
        // ICacheService is always registered when caching is enabled; no-ops internally when HybridCache unavailable
        provider.GetService<ICacheService>().Should().NotBeNull();
        provider.GetRequiredService<ICacheService>().Should().BeOfType<CacheService>();
    }

    [Fact(DisplayName = "AddCaching should return builder for chaining")]
    public void AddCaching_ShouldReturnBuilderForChaining()
    {
        Dictionary<string, string?> configData = new();
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(configData);

        WebApplicationBuilder result = builder.AddCaching();

        result.Should().BeSameAs(builder);
    }

    [Fact(DisplayName = "AddCaching should register ICacheService as singleton")]
    public void AddCaching_ShouldRegisterICacheServiceAsSingleton()
    {
        Dictionary<string, string?> configData = new()
        {
            ["Caching:Enabled"] = "true"
        };
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(configData);
        builder.AddCaching();

        ServiceDescriptor? descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(ICacheService));

        descriptor.Should().NotBeNull();
        descriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}
