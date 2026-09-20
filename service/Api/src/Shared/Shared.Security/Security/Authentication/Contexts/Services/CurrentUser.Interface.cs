namespace Shared.Security.Authentication.Contexts.Services;

/// <summary>
/// Provides access to the current authenticated user's information.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the identifier of the current user.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the email address of the current user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the IP address of the current request.
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the session identifier for guest (unauthenticated) users.
    /// Resolved from the configured guest session cookie or "X-Session-Id" header.
    /// </summary>
    string? SessionId { get; }

    /// <summary>
    /// Gets the device/user agent of the current request.
    /// </summary>
    string? Device { get; }
}
