using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles.Claims;
using Shared.Security.Identity.Domain.Users.Claims;
using Shared.Security.Identity.Domain.Users.Roles;

namespace Shared.Security.Authorization.Permissions.Store;

/// <summary>
/// Retrieves permissions for a user by merging role claims and user claims from the database.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="PermissionStoreService"/>.
/// </remarks>
/// <param name="dbContext">The database context.</param>
/// <param name="logger">The logger instance.</param>
public sealed partial class PermissionStoreService(
    IApplicationDbContext dbContext,
    ILogger<PermissionStoreService> logger) : IPermissionStore
{
    /// <summary>
    /// The database context for accessing permission data.
    /// </summary>
    private readonly IApplicationDbContext _dbContext = dbContext;

    /// <summary>
    /// Logger for tracking store operations.
    /// </summary>
    private readonly ILogger<PermissionStoreService> _logger = logger;

    /// <inheritdoc />
    public async Task<Result<HashSet<string>>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Receive: Fetch claims associated with all roles assigned to the user
            List<string?> roleClaims = await _dbContext.Set<UserRole>()
                .Where(ur => ur.UserId == userId)
                .Join(
                    _dbContext.Set<RoleClaim>(),
                    ur => ur.RoleId,
                    rc => rc.RoleId,
                    (ur, rc) => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Receive: Fetch claims assigned directly to the specific user account
            List<string?> userClaims = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Transform: Merge role and user claims into a single unique set of permissions
            var allPermissions = roleClaims
                .Concat(userClaims)
                .Where(p => p != null)
                .Select(p => p!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: Record quantity of permissions loaded for the user
            Loggers.LogPermissionsLoaded(_logger, allPermissions.Count, userId);

            return Result<HashSet<string>>.Ok(allPermissions, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Log: Detailed error for database query failure
            Loggers.LogGetPermissionsFailed(_logger, userId, ex);

            // Fallback: Return empty permission set to prevent complete system failure
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <inheritdoc />
    public async Task<Result<HashSet<string>>> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken ct = default)
    {
        try
        {
            // Receive: Fetch claims associated with the specific role
            List<string?> roleClaims = await _dbContext.Set<RoleClaim>()
                .Where(rc => rc.RoleId == roleId)
                .Select(rc => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Transform: Filter and clean permission strings
            var permissions = roleClaims
                .Where(p => p != null)
                .Select(p => p!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: Record quantity of permissions loaded for the role
            Loggers.LogRolePermissionsLoaded(_logger, permissions.Count, roleId);

            return Result<HashSet<string>>.Ok(permissions, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Log: Detailed error for database query failure
            Loggers.LogGetRolePermissionsFailed(_logger, roleId, ex);

            // Fallback: Return empty permission set
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <inheritdoc />
    public async Task<Result<HashSet<Guid>>> GetUserRoleIdsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            List<Guid> roleIds = await _dbContext.Set<UserRole>()
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync(ct);

            return Result<HashSet<Guid>>.Ok(roleIds.ToHashSet(), PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Log: Failed to get roles for user
            Loggers.LogGetPermissionsFailed(_logger, userId, ex);
            return Result<HashSet<Guid>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <inheritdoc />
    public async Task<Result<HashSet<string>>> GetUserDirectPermissionsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Receive: Fetch claims assigned directly to the specific user account
            List<string?> userClaims = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Transform: Filter and clean permission strings
            var permissions = userClaims
                .Where(p => p != null)
                .Select(p => p!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: Record quantity of direct permissions loaded
            Loggers.LogPermissionsLoaded(_logger, permissions.Count, userId);

            return Result<HashSet<string>>.Ok(permissions, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Log: Detailed error for database query failure
            Loggers.LogGetPermissionsFailed(_logger, userId, ex);

            // Fallback: Return empty set
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <inheritdoc />
    public async Task<Result> AddRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        try
        {
            var claims = permissions.Select(p => new RoleClaim
            {
                RoleId = roleId,
                ClaimType = PermissionMetadataConstant.ClaimType,
                ClaimValue = p
            }).ToList();

            if (claims.Count == 0) return Result.Ok();

            await _dbContext.Set<RoleClaim>().AddRangeAsync(claims, ct);
            await _dbContext.SaveChangesAsync(ct);

            Loggers.LogBatchAddRolePermissions(_logger, claims.Count, roleId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Loggers.LogBatchAddRolePermissionsFailed(_logger, roleId, ex.Message);
            return PermissionStoreResult.Failure.Unexpected(PermissionStoreConstant.Patterns.BatchAddFailed, "Failed to batch add permissions to role.");
        }
    }

    /// <inheritdoc />
    public async Task<Result> RemoveRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        try
        {
            var permissionSet = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (permissionSet.Count == 0) return Result.Ok();

            List<RoleClaim> claimsToRemove = await _dbContext.Set<RoleClaim>()
                .Where(rc => rc.RoleId == roleId && rc.ClaimType == PermissionMetadataConstant.ClaimType && permissionSet.Contains(rc.ClaimValue!))
                .ToListAsync(ct);

            if (claimsToRemove.Count == 0) return Result.Ok();

            _dbContext.Set<RoleClaim>().RemoveRange(claimsToRemove);
            await _dbContext.SaveChangesAsync(ct);

            Loggers.LogBatchRemoveRolePermissions(_logger, claimsToRemove.Count, roleId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Loggers.LogBatchRemoveRolePermissionsFailed(_logger, roleId, ex.Message);
            return PermissionStoreResult.Failure.Unexpected(PermissionStoreConstant.Patterns.BatchRemoveFailed, "Failed to batch remove permissions from role.");
        }
    }

    /// <inheritdoc />
    public async Task<Result> AddUserDirectPermissionsAsync(Guid userId, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        try
        {
            var claims = permissions.Select(p => new UserClaim
            {
                UserId = userId,
                ClaimType = PermissionMetadataConstant.ClaimType,
                ClaimValue = p
            }).ToList();

            if (claims.Count == 0) return Result.Ok();

            await _dbContext.Set<UserClaim>().AddRangeAsync(claims, ct);
            await _dbContext.SaveChangesAsync(ct);

            Loggers.LogBatchAddUserPermissions(_logger, claims.Count, userId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Loggers.LogBatchAddUserPermissionsFailed(_logger, userId, ex.Message);
            return PermissionStoreResult.Failure.Unexpected(PermissionStoreConstant.Patterns.BatchAddFailed, "Failed to batch add direct permissions to user.");
        }
    }

    /// <inheritdoc />
    public async Task<Result> RemoveUserDirectPermissionsAsync(Guid userId, IEnumerable<string> permissions, CancellationToken ct = default)
    {
        try
        {
            var permissionSet = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (permissionSet.Count == 0) return Result.Ok();

            List<UserClaim> claimsToRemove = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.UserId == userId && uc.ClaimType == PermissionMetadataConstant.ClaimType && permissionSet.Contains(uc.ClaimValue!))
                .ToListAsync(ct);

            if (claimsToRemove.Count == 0) return Result.Ok();

            _dbContext.Set<UserClaim>().RemoveRange(claimsToRemove);
            await _dbContext.SaveChangesAsync(ct);

            Loggers.LogBatchRemoveUserPermissions(_logger, claimsToRemove.Count, userId);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Loggers.LogBatchRemoveUserPermissionsFailed(_logger, userId, ex.Message);
            return PermissionStoreResult.Failure.Unexpected(PermissionStoreConstant.Patterns.BatchRemoveFailed, "Failed to batch remove direct permissions from user.");
        }
    }

    /// <inheritdoc />
    // Batch: Load all distinct permission identifiers across role + user claim tables.
    public async Task<Result<HashSet<string>>> GetAllPermissionIdentifiersAsync(CancellationToken ct = default)
    {
        try
        {
            // Batch: Query distinct permission claim values from role claims.
            List<string?> roleIdentifiers = await _dbContext.Set<RoleClaim>()
                .Where(rc => rc.ClaimType == PermissionMetadataConstant.ClaimType)
                .Select(rc => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Batch: Query distinct permission claim values from user claims.
            List<string?> userIdentifiers = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.ClaimType == PermissionMetadataConstant.ClaimType)
                .Select(uc => uc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Merge: Combine role and user identifiers into single unique set.
            var identifiers = roleIdentifiers
                .Concat(userIdentifiers)
                .Where(v => v != null)
                .Select(v => v!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: Record total distinct permission identifiers loaded from store.
            Loggers.LogAllIdentifiersLoaded(_logger, identifiers.Count);

            return Result<HashSet<string>>.Ok(identifiers, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Catch: Return empty set on failure to prevent cascading errors.
            Loggers.LogGetPermissionsFailed(_logger, Guid.Empty, ex);
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }
}
