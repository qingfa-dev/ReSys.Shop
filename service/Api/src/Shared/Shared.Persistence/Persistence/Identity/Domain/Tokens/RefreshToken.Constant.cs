namespace Shared.Security.Identity.Domain.Tokens;

/// <summary>
/// Contains constant values for the RefreshToken domain.
/// </summary>
public static class RefreshTokenConstant
{
    /// <summary>
    /// Validation constraints for refresh token properties.
    /// </summary>
    public static class Constraints
    {
        public static class TokenHash
        {
            public const int MaxLength = 500;
        }
    }

        /// <summary>
    /// Standard reasons for token revocation.
    /// </summary>
    public static class RevocationReasons
    {
        public const string Replaced = "replaced";
        public const string UserLogout = "user_logout";
        public const string UserLogoutAll = "user_logout_all";
        public const string ReuseDetected = "reuse_detected";
    }
}
