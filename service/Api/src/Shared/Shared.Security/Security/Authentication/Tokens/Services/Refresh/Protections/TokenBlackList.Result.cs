namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

public static class TokenBlacklistResult
{
    /// <summary>
    /// Success result for token security operations.
    /// </summary>
    public static class Success
    {
        /// <summary>
        /// Indicates the token is not blacklisted.
        /// </summary>
        public static Result Blacklisted => Result.Ok();

        /// <summary>
        /// Indicates token reuse was not detected.
        /// </summary>
        public static Result<bool> NoReuseDetected => Result<bool>.Ok(false);
    }

    /// <summary>
    /// Token security related Error.
    /// </summary>
    public static class Failure
    {
        /// <summary>
        /// Token is blacklisted.
        /// </summary>
        public static Error NotBlacklisted => Error.Validation(
            code: "TokenSecurity.Blacklisted",
            message: "Token is not blacklisted.");

        /// <summary>
        /// Failed to check token blacklist.
        /// </summary>
        public static Error BlacklistCheckFailed => Error.Unexpected(
            code: "TokenSecurity.BlacklistCheckFailed",
            message: "Failed to check token blacklist.");

        /// <summary>
        /// Failed to blacklist token.
        /// </summary>
        public static Error BlacklistFailed => Error.Unexpected(
            code: "TokenSecurity.BlacklistFailed",
            message: "Failed to blacklist token.");

        /// <summary>
        /// Token theft detection failed due to infrastructure error.
        /// </summary>
        public static Error TheftDetectionFailed => Error.Unexpected(
            code: "TokenSecurity.TheftDetectionFailed",
            message: "Failed to detect token theft due to infrastructure Error.");

        /// <summary>
        /// Token theft detector failed due to infrastructure error.
        /// </summary>
        public static Error TheftDetectorError => Error.Unexpected(
            code: "TokenSecurity.TheftDetectorError",
            message: "Failed to detect token reuse due to infrastructure Error.");
    }
}