using System.Threading.RateLimiting;

using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shared.Application.Extensions.Validations;
using Shared.Security.RateLimiting.Options;

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
        builder.Services.AddSingleton<IValidator<RateLimitSetting>, RateLimitSettingValidator>();

        builder.Services.AddOptions<RateLimitSetting>()
            .BindConfiguration(RateLimitSetting.SectionName)
            .ValidateFluentValidation()
            .ValidateOnStart();

        RateLimitSetting setting = builder.Configuration.GetSection(RateLimitSetting.SectionName).Get<RateLimitSetting>() ?? new();

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            if (!setting.Enabled)
                return;

            ConfigurePolicies(options, setting);

            return;
        });

        return builder;
    }

    private static void ConfigurePolicies(RateLimiterOptions options, RateLimitSetting setting)
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ =>
                {
                    var cfg = setting.Policies.GetValueOrDefault(DefaultPolicy, new RateLimitPolicyConfig());
                    return new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = cfg.PermitLimit,
                        Window = TimeSpan.FromSeconds(cfg.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    };
                }));

        AddNamedPolicy(options, AuthPolicy, setting.Policies, ipPartition: true);
        AddNamedPolicy(options, RegisterPolicy, setting.Policies, ipPartition: true);
        AddNamedPolicy(options, ForgotPasswordPolicy, setting.Policies, ipPartition: true);
        AddNamedPolicy(options, PaymentPolicy, setting.Policies, ipPartition: true, userPartition: true);
    }

    private static void AddNamedPolicy(
        RateLimiterOptions options,
        string policyName,
        Dictionary<string, RateLimitPolicyConfig> policies,
        bool ipPartition = false,
        bool userPartition = false)
    {
        options.AddPolicy(policyName, httpContext =>
        {
            var key = (ipPartition ? httpContext.Connection.RemoteIpAddress?.ToString() : null)
                ?? (userPartition ? httpContext.User.Identity?.Name : null)
                ?? "unknown";

            var cfg = policies.GetValueOrDefault(policyName, new RateLimitPolicyConfig());

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
}
