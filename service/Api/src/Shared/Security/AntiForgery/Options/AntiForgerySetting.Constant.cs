namespace Shared.Security.AntiForgery.Options;

public static class AntiForgerySettingConstant
{
    public static class Defaults
    {
        public const bool IsEnabled = true;

        public const string HeaderName = "X-XSRF-TOKEN";

        public const bool Required = true;

        public const string CookieName = "XSRF-TOKEN";

        public const string CookieSameSite = "Strict";

        public const string CookieSecurePolicy = "Always";

        public const bool CookieHttpOnly = true;

        public static readonly int? CookieMaxAgeMinutes;
    }

    public static class Constraints
    {
        public const int HeaderNameMinLength = 1;

        public const int HeaderNameMaxLength = 256;

        public const int CookieNameMinLength = 1;

        public const int CookieNameMaxLength = 256;
    }
}
