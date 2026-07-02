using Google.Apis.Auth;

using Shared.Security.Authentication.External.Models;

namespace Shared.Security.Authentication.External.Providers;

/// <summary>
/// Strategy pattern interface for pluggable external OAuth login providers.
/// </summary>
public interface IExternalLoginProvider
{
    /// <summary>Provider name key used for routing (e.g. "google").</summary>
    string Provider { get; }

    /// <summary>
    /// Returns safe, public-facing provider configuration for the frontend.
    /// </summary>
    ProviderOption GetProviderConfig();

    /// <summary>
    /// Validates an ID token issued by the external provider and returns user info.
    /// </summary>
    /// <param name="idToken">The ID token from the OAuth provider.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing ExternalUserInfo on success, or failure with error details.</returns>
    /// <exception cref="InvalidJwtException">Thrown when the token is invalid or tampered.</exception>
    // Contract: pre=!string.IsNullOrEmpty(idToken), post=result.IsSuccess||result.IsFailure,
    //           throws=InvalidJwtException
    Task<Result<ExternalUserInfo>> ValidateIdTokenAsync(string idToken, CancellationToken ct = default);
}