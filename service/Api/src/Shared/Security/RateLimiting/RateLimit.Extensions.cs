using System.Threading.RateLimiting;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Security.RateLimiting;

public static class RateLimitExtensions
{
    public const string AuthPolicy = "auth";
    public const string RegisterPolicy = "register";
    public const string ForgotPasswordPolicy = "forgot-password";
    public const string PaymentPolicy = "payment";
    public const string DefaultPolicy = "default";

    public static WebApplicationBuilder AddRateLimiting(this WebApplicationBuilder builder)
    {
        var policies = builder.Configuration.GetSection("RateLimit:Policies").Get<Dictionary<string, RateLimitPolicyConfig>>()
            ?? new Dictionary<string, RateLimitPolicyConfig>();

        var defaultConfig = policies.GetValueOr(DefaultPolicy, new RateLimitPolicyConfig { PermitLimit = 100, WindowSeconds = 60 });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = defaultConfig.PermitLimit,
                        Window = TimeSpan.FromSeconds(defaultConfig.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            AddNamedPolicy(options, AuthPolicy, policies, ipPartition: true);
            AddNamedPolicy(options, RegisterPolicy, policies, ipPartition: true);
            AddNamedPolicy(options, ForgotPasswordPolicy, policies, ipPartition: true);
            AddNamedPolicy(options, PaymentPolicy, policies, ipPartition: true, userPartition: true);
        });

        return builder;
    }

    private static void AddNamedPolicy(
        RateLimiterOptions options,
        string policyName,
        Dictionary<string, RateLimitPolicyConfig> policies,
        bool ipPartition = false,
        bool userPartition = false)
    {
        var cfg = policies.GetValueOr(policyName, new RateLimitPolicyConfig { PermitLimit = 10, WindowSeconds = 60 });
        options.AddPolicy(policyName, httpContext =>
        {
            var key = (ipPartition ? httpContext.Connection.RemoteIpAddress?.ToString() : null)
                ?? (userPartition ? httpContext.User.Identity?.Name : null)
                ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = cfg.PermitLimit,
                Window = TimeSpan.FromSeconds(cfg.WindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0,
                AutoReplenishment = true
            });
        });
    }

    public sealed class RateLimitPolicyConfig
    {
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
    }
}

internal static class DictionaryExtensions
{
    internal static TValue GetValueOr<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        where TKey : notnull
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
