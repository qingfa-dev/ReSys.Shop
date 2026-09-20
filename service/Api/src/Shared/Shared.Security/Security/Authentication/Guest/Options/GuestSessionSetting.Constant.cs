namespace Shared.Security.Authentication.Guest.Options;

public static class GuestSessionSettingConstant
{
    public static class Defaults
    {
        public const string CookieName = "session_id";

        public const string CookieSameSite = "Lax";

        public const string CookieSecurePolicy = "Always";

        public const bool CookieHttpOnly = true;

        public const int ExpirationInDays = 365;
    }

    public static class Constraints
    {
        public const int CookieNameMinLength = 1;

        public const int CookieNameMaxLength = 256;

        public const int ExpirationInDaysMin = 1;

        public const int ExpirationInDaysMax = 3650;
    }
}
