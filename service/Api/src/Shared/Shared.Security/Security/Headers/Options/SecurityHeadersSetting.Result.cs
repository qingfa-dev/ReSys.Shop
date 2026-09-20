namespace Shared.Security.Headers.Options;

public static class SecurityHeadersResult
{
    public static class Errors
    {
        public static Error ContentSecurityPolicyNotRecommended =>
            Error.Validation(
                "SecurityHeaders.CSP.NotRecommended",
                "Content-Security-Policy is recommended for production. Set a value or explicitly disable via configuration.");
    }
}
