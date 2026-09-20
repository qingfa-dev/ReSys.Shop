namespace Shared.Security.Authentication.Guest.Options;

public static class GuestSessionSettingResult
{
    public static class Failure
    {
        public static Error CookieNameRequired => Error.Validation(
            code: "GuestSession.CookieName.Required",
            message: "GuestSession.CookieName is required");

        public static Error CookieNameInvalid => Error.Validation(
            code: "GuestSession.CookieName.Invalid",
            message: "GuestSession.CookieName must be between 1 and 256 characters");

        public static Error CookieSameSiteInvalid => Error.Validation(
            code: "GuestSession.CookieSameSite.Invalid",
            message: "GuestSession.CookieSameSite must be a valid SameSiteMode value (Unspecified, Lax, Strict, None)");

        public static Error CookieSecurePolicyInvalid => Error.Validation(
            code: "GuestSession.CookieSecurePolicy.Invalid",
            message: "GuestSession.CookieSecurePolicy must be a valid CookieSecurePolicy value (SameAsRequest, Always, None)");

        public static Error CookieHttpOnlyInvalid => Error.Validation(
            code: "GuestSession.CookieHttpOnly.Invalid",
            message: "GuestSession.CookieHttpOnly must be a boolean");

        public static Error ExpirationInDaysInvalid => Error.Validation(
            code: "GuestSession.ExpirationInDays.Invalid",
            message: "GuestSession.ExpirationInDays must be between 1 and 3650");
    }
}
