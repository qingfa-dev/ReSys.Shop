namespace Shared.Security.RateLimiting.Options;

public static class RateLimitResult
{
    public static class Failure
    {
        public static Error PermitLimitZero =>
            Error.Validation(
                "RateLimit.Policies.PermitLimit.Zero",
                "PermitLimit must be greater than zero for all rate limit policies.");

        public static Error WindowSecondsZero =>
            Error.Validation(
                "RateLimit.Policies.WindowSeconds.Zero",
                "WindowSeconds must be greater than zero for all rate limit policies.");
    }
}
