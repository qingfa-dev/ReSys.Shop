namespace Shared.Security.AntiForgery.Options;

public sealed class AntiForgerySetting
{
    public const string SectionName = "AntiForgery";

    public bool IsEnabled { get; set; } = AntiForgerySettingConstant.Defaults.IsEnabled;

    public string HeaderName { get; set; } = AntiForgerySettingConstant.Defaults.HeaderName;

    public bool Required { get; set; } = AntiForgerySettingConstant.Defaults.Required;

    public string CookieName { get; set; } = AntiForgerySettingConstant.Defaults.CookieName;

    public string CookieSameSite { get; set; } = AntiForgerySettingConstant.Defaults.CookieSameSite;

    public string CookieSecurePolicy { get; set; } = AntiForgerySettingConstant.Defaults.CookieSecurePolicy;

    public bool CookieHttpOnly { get; set; } = AntiForgerySettingConstant.Defaults.CookieHttpOnly;

    public int? CookieMaxAgeMinutes { get; set; } = AntiForgerySettingConstant.Defaults.CookieMaxAgeMinutes;
}
