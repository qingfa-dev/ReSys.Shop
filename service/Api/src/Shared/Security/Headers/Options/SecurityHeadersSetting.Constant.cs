namespace Shared.Security.Headers.Options;

public static class SecurityHeadersSettingConstant
{
    public static class Keys
    {
        public const string XContentTypeOptions = "X-Content-Type-Options";

        public const string XFrameOptions = "X-Frame-Options";

        public const string ContentSecurityPolicy = "Content-Security-Policy";

        public const string ReferrerPolicy = "Referrer-Policy";

        public const string PermissionsPolicy = "Permissions-Policy";
    }

    public static class Defaults
    {
        public const bool IsEnabled = true;

        public const string XContentTypeOptions = "nosniff";

        public const string XFrameOptions = "DENY";

        public const string? ContentSecurityPolicy = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'";

        public const string ReferrerPolicy = "strict-origin-when-cross-origin";

        public const string PermissionsPolicy = "camera=(), microphone=(), geolocation=()";
    }
}
