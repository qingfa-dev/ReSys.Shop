namespace Shared.Security.Headers.Options;

public sealed class SecurityHeadersSetting
{
    public bool IsEnabled { get; set; } = SecurityHeadersSettingConstant.Defaults.IsEnabled;

    public string? XContentTypeOptions { get; set; } = SecurityHeadersSettingConstant.Defaults.XContentTypeOptions;

    public string? XFrameOptions { get; set; } = SecurityHeadersSettingConstant.Defaults.XFrameOptions;

    public string? ContentSecurityPolicy { get; set; } = SecurityHeadersSettingConstant.Defaults.ContentSecurityPolicy;

    public string? ReferrerPolicy { get; set; } = SecurityHeadersSettingConstant.Defaults.ReferrerPolicy;

    public string? PermissionsPolicy { get; set; } = SecurityHeadersSettingConstant.Defaults.PermissionsPolicy;
}
