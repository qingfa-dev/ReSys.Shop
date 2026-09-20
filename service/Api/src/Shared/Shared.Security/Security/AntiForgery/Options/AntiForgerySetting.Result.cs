namespace Shared.Security.AntiForgery.Options;

public static class AntiForgerySettingResult
{
    public static class Failure
    {
        public static Error HeaderNameRequired => Error.Validation(
            code: "AntiForgery.HeaderName.Required",
            message: "AntiForgery.HeaderName is required");

        public static Error HeaderNameInvalid => Error.Validation(
            code: "AntiForgery.HeaderName.Invalid",
            message: "AntiForgery.HeaderName must be between 1 and 256 characters");

        public static Error RequiredInvalid => Error.Validation(
            code: "AntiForgery.Required.Invalid",
            message: "AntiForgery.Required must be set");

        public static Error CookieNameRequired => Error.Validation(
            code: "AntiForgery.Cookie.Name.Required",
            message: "AntiForgery.CookieName is required");

        public static Error CookieNameInvalid => Error.Validation(
            code: "AntiForgery.Cookie.Name.Invalid",
            message: "AntiForgery.CookieName must be between 1 and 256 characters");

        public static Error CookieSameSiteInvalid => Error.Validation(
            code: "AntiForgery.Cookie.SameSite.Invalid",
            message: "AntiForgery.CookieSameSite must be a valid SameSiteMode value (Unspecified, Lax, Strict, None)");

        public static Error CookieSecurePolicyInvalid => Error.Validation(
            code: "AntiForgery.Cookie.SecurePolicy.Invalid",
            message: "AntiForgery.CookieSecurePolicy must be a valid CookieSecurePolicy value (SameAsRequest, Always, None)");

        public static Error CookieHttpOnlyInvalid => Error.Validation(
            code: "AntiForgery.Cookie.HttpOnly.Invalid",
            message: "AntiForgery.CookieHttpOnly must be a boolean");

        public static Error CookieMaxAgeMinutesInvalid => Error.Validation(
            code: "AntiForgery.Cookie.MaxAgeMinutes.Invalid",
            message: "AntiForgery.CookieMaxAgeMinutes must be null or a positive integer");
    }
}
