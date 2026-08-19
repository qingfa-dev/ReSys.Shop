namespace Module.Identity.Features.Shared;

public static partial class IdentityFeature
{
    public static class Storefront
    {
        public static class Auth
        {
            public static class Login
            {
                public static class Password
                {
                    public const string Route = "api/storefront/identity/auth/login/password";

                    public const string Description =
                        "Authenticate a store user with email/username/phone and password.";

                    public const string Summary = "Password login";
                }

                public static class External
                {
                    public static class Authenticate
                    {
                        public const string Route = "api/storefront/identity/auth/login/external";

                        public const string Description =
                            "Authenticate a store user using an external provider (Google, etc.) via ID token.";

                        public const string Summary = "External login";
                    }

                    public static class Providers
                    {
                        public const string Route = "api/storefront/identity/auth/login/external/providers";
                        public const string Description = "Retrieve configured external login provider options.";
                        public const string Summary = "External providers";
                    }
                }
            }

            public static class Register
            {
                public const string Route = "api/storefront/identity/auth/register";
                public const string Description = "Register a new user account via email.";
                public const string Summary = "Email register";
            }

            public static class Logout
            {
                public const string Route = "api/storefront/identity/auth/logout";
                public const string Description = "Logout a store user from current device or all devices.";
                public const string Summary = "Logout";
            }

            public static class Sessions
            {
                public static class Get
                {
                    public const string Route = "api/storefront/identity/auth/sessions";
                    public const string Description = "Retrieve the current authenticated user's session.";
                    public const string Summary = "Get session";
                }

                public static class Refresh
                {
                    public const string Route = "api/storefront/identity/auth/sessions/refresh";
                    public const string Description = "Refresh an expired access token using a valid refresh token.";
                    public const string Summary = "Refresh session";
                }
            }
        }

        public static class Passwords
        {
            public static class Change
            {
                public const string Route = "api/storefront/identity/passwords/change";
                public const string Description = "Change user password";
                public const string Summary = "Change password";
            }

            public static class Forgot
            {
                public const string Route = "api/storefront/identity/passwords/forgot";
                public const string Description = "Request password reset";
                public const string Summary = "Forgot password";
            }

            public static class Reset
            {
                public const string Route = "api/storefront/identity/passwords/reset";
                public const string Description = "Reset user password";
                public const string Summary = "Reset password";
            }
        }

        public static class Emails
        {
            public static class Change
            {
                public const string Route = "api/storefront/identity/emails/change";
                public const string Description = "Request email change";
                public const string Summary = "Change email";
            }

            public static class Confirm
            {
                public const string Route = "api/storefront/identity/emails/confirm";
                public const string Description = "Confirm email verification";
                public const string Summary = "Confirm email";
            }

            public static class Resend
            {
                public const string Route = "api/storefront/identity/emails/resend";
                public const string Description = "Resend email verification";
                public const string Summary = "Resend verification";
            }
        }
    }
}
