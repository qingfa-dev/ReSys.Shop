using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Store;

/// <summary>
/// EF Core implementation of <see cref="IRefreshTokenStore"/> for refresh token persistence.
/// </summary>
public partial class RefreshTokenStore(
    IApplicationDbContext dbContext,
    ILogger<RefreshTokenStore> logger) : IRefreshTokenStore
{
    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        // Validate: Guard against empty hash
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        // Call: Retrieve token by SHA256 hash from the identity store
        try
        {
            return await dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);
        }
        catch (Exception ex)
        {
            Loggers.LogStoreOperationFailed(logger, nameof(GetByTokenHashAsync), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Call: Retrieve token by primary key
        try
        {
            return await dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Id == id, ct);
        }
        catch (Exception ex)
        {
            Loggers.LogStoreOperationFailed(logger, nameof(GetByIdAsync), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        // Contract: Returns only non-expired, non-revoked tokens for the given user
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Call: Query active tokens with user and time filters
        try
        {
            return await dbContext.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > now)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            Loggers.LogStoreOperationFailed(logger, nameof(GetActiveByUserIdAsync), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task AddAsync(RefreshToken entity, CancellationToken ct = default)
    {
        // Create: Register entity with EF Core change tracker
        try
        {
            dbContext.Set<RefreshToken>().Add(entity);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            Loggers.LogStoreOperationFailed(logger, nameof(AddAsync), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(RefreshToken entity, CancellationToken ct = default)
    {
        // Update: EF Core change tracker detects modifications on tracked entities
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            Loggers.LogStoreOperationFailed(logger, nameof(UpdateAsync), ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        // Persist: Flush all pending changes in a single unit-of-work
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            Loggers.LogStoreOperationFailed(logger, nameof(SaveChangesAsync), ex);
            throw;
        }
    }
}
