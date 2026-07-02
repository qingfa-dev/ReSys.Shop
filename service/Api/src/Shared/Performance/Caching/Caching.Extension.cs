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
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration for retrieving settings.</param>
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
            .ValidateFluentValidation();
        
        #endregion

        #region Cache Infrastructure

        // Check: Global enablement status for caching infrastructure
        CachingSetting cachingSetting = builder.Configuration.GetSection(CachingSetting.SectionName).Get<CachingSetting>() ?? new CachingSetting();

        if (!cachingSetting.Enabled)
        {
            return builder;
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
                // Create: Explicit ConnectionMultiplexer for telemetry instrumentation
                ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(connectionString);
                builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

                // Add: Redis cache registration using ConnectionMultiplexerFactory
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer);
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

        // Add: HybridCache layer combining L1 and L2
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

            // Add: Orchestration service for caching operations
            builder.Services.AddSingleton<ICacheService, CacheService>();
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
