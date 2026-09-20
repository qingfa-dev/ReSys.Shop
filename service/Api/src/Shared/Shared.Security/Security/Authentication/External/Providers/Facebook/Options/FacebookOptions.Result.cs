namespace Shared.Security.Authentication.External.Providers.Facebook.Options;

public static class FacebookOptionsResult
{
    public static class Failure
    {
        public static Error ClientIdRequired => Error.Validation(
            code: "Authentication.Facebook.ClientIdRequired",
            message: "Facebook ClientId is required.");
    }
}
