namespace Shared.Security.Authentication.External.Providers.Microsoft.Options;

public static class MicrosoftOptionsResult
{
    public static class Failure
    {
        public static Error ClientIdRequired => Error.Validation(
            code: "Authentication.Microsoft.ClientIdRequired",
            message: "Microsoft ClientId is required.");
    }
}
