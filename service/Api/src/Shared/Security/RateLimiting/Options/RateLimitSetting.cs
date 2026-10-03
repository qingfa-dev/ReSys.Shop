namespace Shared.Security.RateLimiting.Options;

public sealed class RateLimitSetting
{
    public const string SectionName = "RateLimit";

    public bool Enabled { get; set; } = true;

    public Dictionary<string, RateLimitPolicyConfig> Policies { get; set; } = [];

    public static RateLimitSetting Default => new()
    {
        Enabled = false,
        Policies = new()
        {
            [RateLimitSettingConstant.Allowed.Default] = new RateLimitPolicyConfig { PermitLimit = 1000, WindowSeconds = 60 },
            [RateLimitSettingConstant.Allowed.Auth] = new RateLimitPolicyConfig { PermitLimit = 1000, WindowSeconds = 60 },
            [RateLimitSettingConstant.Allowed.Register] = new RateLimitPolicyConfig { PermitLimit = 1000, WindowSeconds = 3600 },
            [RateLimitSettingConstant.Allowed.ForgotPassword] = new RateLimitPolicyConfig { PermitLimit = 1000, WindowSeconds = 3600 },
            [RateLimitSettingConstant.Allowed.Payment] = new RateLimitPolicyConfig { PermitLimit = 1000, WindowSeconds = 60 },
            [RateLimitSettingConstant.Allowed.Webhook] = new RateLimitPolicyConfig { PermitLimit = 1000, WindowSeconds = 60 }
        }
    };
}

public sealed class RateLimitPolicyConfig
{
    public int PermitLimit { get; set; } = RateLimitSettingConstant.Defaults.PermitLimit;

    public int WindowSeconds { get; set; } = RateLimitSettingConstant.Defaults.WindowSeconds;
}
