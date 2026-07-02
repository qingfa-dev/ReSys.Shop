namespace Shared.Security.Authentication.Guest.Options;

public sealed class GuestSessionSetting
{
    public const string SectionName = "GuestSession";

    public string CookieName { get; set; } = GuestSessionSettingConstant.Defaults.CookieName;

    public string CookieSameSite { get; set; } = GuestSessionSettingConstant.Defaults.CookieSameSite;

    public string CookieSecurePolicy { get; set; } = GuestSessionSettingConstant.Defaults.CookieSecurePolicy;

    public bool CookieHttpOnly { get; set; } = GuestSessionSettingConstant.Defaults.CookieHttpOnly;

    public int ExpirationInDays { get; set; } = GuestSessionSettingConstant.Defaults.ExpirationInDays;
}
