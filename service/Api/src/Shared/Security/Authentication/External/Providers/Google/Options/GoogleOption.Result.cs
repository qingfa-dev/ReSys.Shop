namespace Shared.Security.Authentication.External.Providers.Google.Options;

public static class GoogleOptionsResult
{
    public static class Failure
    {
        public static Error ClientIdRequired => Error.Validation(
            code: "Authentication.Google.ClientIdRequired",
            message: "Google ClientId is required.");
    }
}