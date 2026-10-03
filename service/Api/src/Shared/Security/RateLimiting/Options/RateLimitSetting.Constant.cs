namespace Shared.Security.RateLimiting.Options;

public static class RateLimitSettingConstant
{
    public static class Defaults
    {
        public const bool Enabled = true;

        public const int PermitLimit = 100;

        public const int WindowSeconds = 60;
    }

    public static class Allowed
    {
        public const string Auth = "auth";

        public const string Register = "register";

        public const string ForgotPassword = "forgot-password";

        public const string Payment = "payment";

        public const string Webhook = "webhook";

        public const string Default = "default";
    }
}
