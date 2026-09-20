using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Store;

/// <summary>
/// Store abstraction for refresh token persistence operations.
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>
    /// Retrieves a refresh token by its SHA256 hash.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a refresh token by its unique identifier.
    /// </summary>
    Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all active (non-expired, non-revoked) refresh tokens for a user.
    /// </summary>
    Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new refresh token entity to the store.
    /// </summary>
    Task AddAsync(RefreshToken entity, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing refresh token entity in the store.
    /// </summary>
    Task UpdateAsync(RefreshToken entity, CancellationToken ct = default);

    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
