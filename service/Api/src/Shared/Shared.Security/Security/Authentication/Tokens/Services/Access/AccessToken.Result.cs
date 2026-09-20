namespace Shared.Security.Authentication.Tokens.Services.Access;

/// <summary>
/// Provides centralized success and error cases for access token operations.
/// </summary>
public static class AccessTokenResult
{
    /// <summary>
    /// Access token success results.
    /// </summary>
    public static class Success
    {
        /// <summary>
        /// Indicates the access token was generated successfully.
        /// </summary>
        public static Result Generated => Result.Ok();
    }

    /// <summary>
    /// Access token error cases.
    /// </summary>
    public static class Failure
    {
        /// <summary>
        /// Invalid JWT configuration (missing secret, invalid algorithm, etc.).
        /// </summary>
        public static Error InvalidConfiguration => Error.Unexpected(
            code: "AccessToken.InvalidConfiguration",
            message: "Invalid JWT configuration.");

        /// <summary>
        /// Failed to generate access token due to cryptographic error.
        /// </summary>
        public static Error GenerationFailed => Error.Unexpected(
            code: "AccessToken.GenerationFailed",
            message: "Failed to generate access token.");

        /// <summary>
        /// Failed to validate the generated access token.
        /// </summary>
        public static Error TokenValidationFailed => Error.Unexpected(
            code: "AccessToken.TokenValidationFailed",
            message: "Failed to validate the generated access token.");
    }
}
