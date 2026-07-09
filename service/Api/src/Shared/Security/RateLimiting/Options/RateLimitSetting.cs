namespace Shared.Security.RateLimiting.Options;

public sealed class RateLimitSetting
{
    public const string SectionName = "RateLimit";

    public bool Enabled { get; set; } = true;

    public Dictionary<string, RateLimitPolicyConfig> Policies { get; set; } = [];
}

public sealed class RateLimitPolicyConfig
{
    public int PermitLimit { get; set; } = RateLimitSettingConstant.Defaults.PermitLimit;

    public int WindowSeconds { get; set; } = RateLimitSettingConstant.Defaults.WindowSeconds;
}
