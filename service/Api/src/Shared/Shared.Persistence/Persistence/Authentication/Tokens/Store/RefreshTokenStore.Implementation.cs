using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Tokens;

namespace Shared.Security.Authentication.Tokens.Services.Refresh.Store;

/// <summary>Stores and retrieves refresh tokens using EF Core against the application database context.</summary>
// Invariant: TokenHash is the sole lookup key for GetByTokenHashAsync; revoked tokens keep RevokedAtUtc set; expired tokens are filtered by ExpiresAtUtc.
// Boundary: Store → Persistence — no domain logic lives here, only data access.
public partial class RefreshTokenStore(
    IApplicationDbContext dbContext,
    ILogger<RefreshTokenStore> logger) : IRefreshTokenStore
{
    /// <summary>Looks up a refresh token by its SHA256 hash.</summary>
    // Contract: pre=tokenHash!=null, post=return==null || return.TokenHash==tokenHash, throws=Exception on EF Core failure
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        // Guard: empty hash cannot match any stored token — return null immediately
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        // Call: EF Core query filtered by SHA256 hash (module boundary: Store → Persistence)
        try
        {
            return await dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, ct);
        }
        catch (Exception ex)
        {
            // Catch: log and rethrow to let caller handle infrastructure failure
            Loggers.LogStoreOperationFailed(logger, nameof(GetByTokenHashAsync), ex);
            throw;
        }
    }

    /// <summary>Looks up a refresh token by its primary key.</summary>
    // Contract: pre=id!=Guid.Empty, post=return==null || return.Id==id, throws=Exception on EF Core failure
    public async Task<RefreshToken?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Call: EF Core query by primary key
        try
        {
            return await dbContext.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Id == id, ct);
        }
        catch (Exception ex)
        {
            // Catch: log and rethrow to let caller handle infrastructure failure
            Loggers.LogStoreOperationFailed(logger, nameof(GetByIdAsync), ex);
            throw;
        }
    }

    /// <summary>Returns all non-expired and non-revoked tokens for a user.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return contains only RevokedAtUtc==null && ExpiresAtUtc>UtcNow tokens, throws=Exception on EF Core failure
    public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Call: query tokens filtered by user and active status (module boundary: Store → Persistence)
        try
        {
            return await dbContext.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && rt.RevokedAtUtc == null && rt.ExpiresAtUtc > now)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            // Catch: log and rethrow to let caller handle infrastructure failure
            Loggers.LogStoreOperationFailed(logger, nameof(GetActiveByUserIdAsync), ex);
            throw;
        }
    }

    /// <summary>Persists a new refresh token entity to the database.</summary>
    // Contract: pre=entity!=null, post=entity.Id is generated, throws=Exception on EF Core failure
    public async Task AddAsync(RefreshToken entity, CancellationToken ct = default)
    {
        // Call: register and flush within single unit-of-work (module boundary: Store → Persistence)
        try
        {
            dbContext.Set<RefreshToken>().Add(entity);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Catch: log and rethrow to let caller handle infrastructure failure
            Loggers.LogStoreOperationFailed(logger, nameof(AddAsync), ex);
            throw;
        }
    }

    /// <summary>Saves changes to an existing tracked refresh token entity.</summary>
    // Contract: pre=entity is tracked by DbContext, post=changes persisted, throws=Exception on EF Core failure
    public async Task UpdateAsync(RefreshToken entity, CancellationToken ct = default)
    {
        // Call: EF Core tracks modifications automatically on previously-fetched entities
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Catch: log and rethrow to let caller handle infrastructure failure
            Loggers.LogStoreOperationFailed(logger, nameof(UpdateAsync), ex);
            throw;
        }
    }

    /// <summary>Flushes all pending changes to the database in a single transaction.</summary>
    // Contract: post=all tracked changes persisted, throws=Exception on EF Core failure
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        // Call: flush unit-of-work (module boundary: Store → Persistence)
        try
        {
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Catch: log and rethrow to let caller handle infrastructure failure
            Loggers.LogStoreOperationFailed(logger, nameof(SaveChangesAsync), ex);
            throw;
        }
    }
}
