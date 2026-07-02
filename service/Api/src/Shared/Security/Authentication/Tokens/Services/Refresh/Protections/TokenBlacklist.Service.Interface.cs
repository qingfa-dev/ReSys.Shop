namespace Shared.Security.Authentication.Tokens.Services.Refresh.Protections;

/// <summary>
/// Service for managing token blacklist to support logout and token revocation.
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Checks if a token is blacklisted.
    /// </summary>
    /// <param name="jti">The JWT ID of the token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing a boolean indicating whether the token is blacklisted, or an error.</returns>
    Task<Result> IsBlacklistedAsync(string jti, CancellationToken ct = default);

    /// <summary>
    /// Adds a token to the blacklist.
    /// </summary>
    /// <param name="jti">The JWT ID of the token.</param>
    /// <param name="expiry">The expiration time of the token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or error.</returns>
    Task<Result> BlacklistTokenAsync(string jti, DateTime expiry, CancellationToken ct = default);

    /// <summary>
    /// Cleans up expired entries from the blacklist.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or error.</returns>
    Task<Result> CleanupExpiredAsync(CancellationToken ct = default);
}
