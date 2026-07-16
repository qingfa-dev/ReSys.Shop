using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shared.Application.Extensions.Validations;
using Shared.Performance.Caching.Options;
using Shared.Performance.Caching.Wrappers;

using StackExchange.Redis;

namespace Shared.Performance.Caching;

/// <summary>
/// Provides extension methods for configuring caching infrastructure including Hybrid, Memory, and Distributed caches.
/// </summary>
public static class CachingExtensions
{
    #region Service Registration

    /// <summary>
    /// Registers caching services with the specified configuration.
    /// </summary>
    /// <returns>The service collection for method chaining.</returns>
    /// <exception cref="OptionsValidationException">
    /// Thrown when caching options fail validation on startup.
    /// </exception>
    public static WebApplicationBuilder AddCaching(this WebApplicationBuilder builder)
    {
        #region Options Configuration
        // Add: Validation requirements for caching options
        builder.Services.AddScoped<IValidator<CachingSetting>, CachingSettingValidator>();

        // Initialize: Fluent options builder for caching configuration
        builder.Services.AddOptions<CachingSetting>()
            .BindConfiguration(CachingSetting.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();
        
        #endregion

        #region Cache Infrastructure

        // Check: Global enablement status for caching infrastructure
        CachingSetting cachingSetting = builder.Configuration.GetSection(CachingSetting.SectionName).Get<CachingSetting>() ?? new CachingSetting();

        // Add: ICacheService always registered — CacheService checks CachingSetting.Enabled internally and no-ops when disabled.
        // This ensures dependents (e.g. PermissionCache) are always resolvable regardless of caching configuration.
        builder.Services.AddSingleton<ICacheService, CacheService>();

        if (!cachingSetting.Enabled)
        {
            return builder;
        }

        // Add: HybridCache layer combining L1 and L2 (optional — only registered when hybrid mode is enabled)
        if (cachingSetting.Hybrid.Enabled)
        {
            builder.Services.AddHybridCache(options =>
            {
                options.MaximumPayloadBytes = cachingSetting.Hybrid.MaximumPayloadBytes;
                options.MaximumKeyLength = cachingSetting.Hybrid.MaximumKeyLength;
                options.DefaultEntryOptions = new CachingEntryOption
                {
                    Expiration = TimeSpan.FromMinutes(cachingSetting.Hybrid.DefaultExpirationMinutes),
                    LocalCacheExpiration = TimeSpan.FromMinutes(cachingSetting.Memory.DefaultExpirationMinutes)
                }.ToHybridCacheEntryOptions();
            });
        }

        // Add: In-memory cache with configured compaction
        if (cachingSetting.Memory.Enabled)
        {
            builder.Services.AddMemoryCache(options =>
            {
                options.CompactionPercentage = cachingSetting.Memory.CompactionPercentage / 100.0;
            });
        }

        // Add: Distributed cache provider (Redis or memory fallback)
        if (cachingSetting.Distributed.Enabled &&
            cachingSetting.Distributed.Type.Equals("redis", StringComparison.OrdinalIgnoreCase))
        {
            // Initialize: Redis connection using centralized constants with fallback
            (string _, string? connectionString) = ResolveConnectionString(builder.Configuration);

            if (connectionString is null)
            {
                // Fallback: No Redis connection string configured, use in-memory distributed cache
                builder.Services.AddDistributedMemoryCache();
            }
            else
            {
                // Register: IConnectionMultiplexer as lazy singleton — connection is deferred
                // to first use so the application can start when Redis is temporarily unavailable.
                // AbortOnConnectFail=false prevents Connect() from throwing; the multiplexer will
                // keep retrying in the background and become usable once Redis recovers.
                builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                {
                    var configOptions = ConfigurationOptions.Parse(connectionString);
                    configOptions.AbortOnConnectFail = false;
                    configOptions.ConnectTimeout = 5000;
                    return ConnectionMultiplexer.Connect(configOptions);
                });

                // Add: Redis cache — StackExchangeRedisCache connects lazily on first cache operation
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = connectionString;
                });

                // Initialize: Configure instance name for cache key isolation
                builder.Services.AddOptions<RedisCacheOptions>()
                    .Configure((options) =>
                    {
                        options.InstanceName = "ReSys_";
                    });
            }
        }
        else
        {
            // Add: Distributed memory cache fallback
            builder.Services.AddDistributedMemoryCache();
        }

        #endregion

        return builder;
    }

    #endregion

    #region Private Helpers

    private static (string Name, string? Value) ResolveConnectionString(
        Microsoft.Extensions.Configuration.ConfigurationManager configuration)
    {
        var aspireConnectionString = configuration.GetConnectionString(CachingSettingConstant.Aspire);
        if (!string.IsNullOrEmpty(aspireConnectionString))
        {
            return (CachingSettingConstant.Aspire, aspireConnectionString);
        }

        var defaultConnectionString = configuration.GetConnectionString(CachingSettingConstant.Default);
        if (!string.IsNullOrEmpty(defaultConnectionString))
        {
            return (CachingSettingConstant.Default, defaultConnectionString);
        }

        return (CachingSettingConstant.Aspire, null);
    }

    #endregion
}
