using Shared.Security.Authentication.Tokens.Models;

namespace Shared.Security.Authentication.Tokens.Services.Refresh;

/// <summary>
/// Service for managing refresh tokens including generation, retrieval, and revocation.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new refresh token for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the refresh token response or error details.</returns>
    Task<Result<RefreshTokenResponseModel>> GenerateAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a refresh token by its value.
    /// </summary>
    /// <param name="token">The refresh token value.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the refresh token response or error details.</returns>
    Task<Result<RefreshTokenResponseModel>> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Revokes a refresh token based on the provided request.
    /// </summary>
    /// <param name="request">The revocation request containing the token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or error details.</returns>
    Task<Result> RevokeAsync(RevokeTokenRequestModel request, CancellationToken ct = default);

    /// <summary>
    /// Revokes all refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="reason">The reason for revoking all tokens.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or error details.</returns>
    Task<Result<int>> RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct = default);

    /// <summary>
    /// Rotates a refresh token by validating the old one and issuing a new one.
    /// Handles revocation of the old token and linking it to the new one.
    /// </summary>
    /// <param name="token">The current refresh token value.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the new refresh token response or error details.</returns>
    Task<Result<RefreshTokenResponseModel>> RotateAsync(string token, CancellationToken ct = default);
}
