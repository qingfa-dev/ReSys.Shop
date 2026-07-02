namespace Shared.Security.Authentication.Tokens.Options;

/// <summary>
/// Contains constant values for JWT settings.
/// </summary>
public static class JwtSettingsConstant
{
    /// <summary>
    /// Default values for JWT settings.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// Default JWT algorithm.
        /// </summary>
        public const string Algorithm = "HS256";
        
        /// <summary>
        /// Default access token expiration in minutes (15 minutes).
        /// </summary>
        public const int AccessTokenExpirationInMinutes = 15;
        
        /// <summary>
        /// Default refresh token expiration in days (7 days).
        /// </summary>
        public const int RefreshTokenExpirationInDays = 7;
    }

    /// <summary>
    /// Validation constraints for JWT settings.
    /// </summary>
    public static class Constraints
    {
        /// <summary>
        /// Constraints for JWT secret.
        /// </summary>
        public static class Secret
        {
            /// <summary>
            /// Minimum length for HS256 algorithm.
            /// </summary>
            public const int MinLength = 32;
        }

        /// <summary>
        /// Constraints for access token expiration.
        /// </summary>
        public static class AccessTokenExpiration
        {
            /// <summary>
            /// Maximum expiration in minutes (24 hours).
            /// </summary>
            public const int MaxMinutes = 1440;
        }

        /// <summary>
        /// Constraints for refresh token expiration.
        /// </summary>
        public static class RefreshTokenExpiration
        {
            /// <summary>
            /// Maximum expiration in days (365 days).
            /// </summary>
            public const int MaxDays = 365;
        }

        /// <summary>
        /// Constraints for token security.
        /// </summary>
        public static class TokenSecurity
        {
            /// <summary>
            /// Default max token age in days.
            /// </summary>
            public const int DefaultMaxTokenAgeDays = 30;
        }
    }

    /// <summary>
    /// Allowed values and patterns for JWT settings.
    /// </summary>
    public static class Allowed
    {
        /// <summary>
        /// Allowed JWT algorithms.
        /// </summary>
        public static readonly string[] Algorithms = ["HS256", "RS256", "ES256"];
        
        /// <summary>
        /// Weak secrets that should be flagged in production.
        /// </summary>
        public static readonly string[] WeakSecrets = 
        [
            "SuperSecretKeyForTestingPurposesOnly123!",
            "secret",
            "123456",
            "password",
            "admin",
            "test",
            "default"
        ];
    }
}