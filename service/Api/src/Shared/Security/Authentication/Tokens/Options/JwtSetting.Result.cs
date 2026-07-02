namespace Shared.Security.Authentication.Tokens.Options;



/// <summary>
/// Contains result definitions for JWT settings validation.
/// </summary>
public static class JwtSettingsResult
{
    /// <summary>
    /// Success messages for JWT settings.
    /// </summary>
    public static class Success
    {
        public const string Valid = "JWT settings are valid.";
        public const string WeakSecretWarning = "JWT settings are valid but use a weak or default secret.";
    }

    /// <summary>
    /// Failure errors for JWT settings validation.
    /// </summary>
    public static class Failure
    {
        public static Error SettingsNull => Error.Validation(
            code: "Jwt.Settings.Null",
            message: "JWT settings configuration is null.");

        public static Error SecretRequired => Error.Validation(
            code: "Jwt.Secret.Required",
            message: "JWT secret is required.");

        public static Error SecretTooShort => Error.Validation(
            code: "Jwt.Secret.TooShort",
            message: $"JWT secret must be at least {JwtSettingsConstant.Constraints.Secret.MinLength} characters for HS256 algorithm.");

        public static Error IssuerRequired => Error.Validation(
            code: "Jwt.Issuer.Required",
            message: "JWT issuer is required.");

        public static Error AudienceRequired => Error.Validation(
            code: "Jwt.Audience.Required",
            message: "JWT audience is required.");

        public static Error AlgorithmRequired => Error.Validation(
            code: "Jwt.Algorithm.Required",
            message: "JWT algorithm is required.");

        public static Error AlgorithmNoneNotAllowed => Error.Validation(
            code: "Jwt.Algorithm.NoneNotAllowed",
            message: "JWT algorithm 'none' is not allowed - this is a critical security vulnerability.");

        public static Error AlgorithmNotAllowed => Error.Validation(
            code: "Jwt.Algorithm.NotAllowed",
            message: $"JWT algorithm must be one of: {string.Join(", ", JwtSettingsConstant.Allowed.Algorithms)}.");

        public static Error AccessTokenExpirationInvalid => Error.Validation(
            code: "Jwt.AccessTokenExpiration.Invalid",
            message: "Access token expiration must be greater than 0 minutes.");

        public static Error AccessTokenExpirationExceeded => Error.Validation(
            code: "Jwt.AccessTokenExpiration.Exceeded",
            message: $"Access token expiration cannot exceed {JwtSettingsConstant.Constraints.AccessTokenExpiration.MaxMinutes} minutes (24 hours).");

        public static Error RefreshTokenExpirationInvalid => Error.Validation(
            code: "Jwt.RefreshTokenExpiration.Invalid",
            message: "Refresh token expiration must be greater than 0 days.");

        public static Error RefreshTokenExpirationExceeded => Error.Validation(
            code: "Jwt.RefreshTokenExpiration.Exceeded",
            message: $"Refresh token expiration cannot exceed {JwtSettingsConstant.Constraints.RefreshTokenExpiration.MaxDays} days.");

        public static Error MaxTokenAgeRequired => Error.Validation(
            code: "Jwt.TokenSecurity.MaxTokenAge.Required",
            message: "Max token age must be greater than 0 days when reuse detection is enabled.");

        public static Error WeakSecretWarning => Error.Validation(
            code: "Jwt.Secret.Weak",
            message: "JWT secret appears to be weak or a default value. This should be changed for production environments.");
    }
}