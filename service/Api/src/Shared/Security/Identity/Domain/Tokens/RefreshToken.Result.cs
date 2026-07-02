namespace Shared.Security.Identity.Domain.Tokens;

public static class RefreshTokenResult
{
    /// <summary>
    /// Refresh token success results.
    /// </summary>
    public static class Success
    {
        /// <summary>
        /// Indicates the refresh token was generated successfully.
        /// </summary>
        public static Result Generated => Result.Ok();

        /// <summary>
        /// Indicates the refresh token was revoked successfully.
        /// </summary>
        public static Result Revoked => Result.Ok();

        /// <summary>
        /// Indicates all refresh tokens were revoked for a user.
        /// </summary>
        public static Result<int> AllRevoked(int count) => Result<int>.Ok(count);
    }

    /// <summary>
    /// Refresh token related Error.
    /// </summary>
    public static class Failure
    {
        /// <summary>
        /// Refresh token not found in the store.
        /// </summary>
        public static Error NotFound => Error.NotFound(
            code: "RefreshToken.NotFound",
            message: "Refresh token not found.");

        /// <summary>
        /// Refresh token has expired.
        /// </summary>
        public static Error Expired => Error.Validation(
            code: "RefreshToken.Expired",
            message: "Refresh token has expired.");

        /// <summary>
        /// Refresh token has been revoked.
        /// </summary>
        public static Error Revoked => Error.Validation(
            code: "RefreshToken.Revoked",
            message: "Refresh token has been revoked.");

        /// <summary>
        /// Token reuse detected (potential theft).
        /// </summary>
        public static Error TheftDetected => Error.Validation(
            code: "RefreshToken.TheftDetected",
            message: "Token reuse detected. All user tokens have been revoked.");

        /// <summary>
        /// Failed to generate refresh token.
        /// </summary>
        public static Error GenerationFailed => Error.Unexpected(
            code: "RefreshToken.GenerationFailed",
            message: "Failed to generate refresh token.");

        /// <summary>
        /// Failed to rotate refresh token.
        /// </summary>
        public static Error RotationFailed => Error.Unexpected(
            code: "RefreshToken.RotationFailed",
            message: "Failed to rotate refresh token.");

        /// <summary>
        /// Failed to revoke refresh token.
        /// </summary>
        public static Error RevocationFailed => Error.Unexpected(
            code: "RefreshToken.RevocationFailed",
            message: "Failed to revoke refresh token.");

        /// <summary>
        /// Token theft detection infrastructure failure.
        /// </summary>
        public static Error TokenTheftDetectorFailure => Error.Unexpected(
            code: "RefreshToken.TokenTheftDetectorFailure",
            message: "Failed to detect token reuse due to infrastructure error.");

        /// <summary>
        /// Refresh token store access failure.
        /// </summary>
        public static Error StoreFailure => Error.Unexpected(
            code: "RefreshToken.StoreFailure",
            message: "Failed to access refresh token store.");
    }
}