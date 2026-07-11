using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles.Claims;
using Shared.Security.Identity.Domain.Users.Claims;
using Shared.Security.Identity.Domain.Users.Roles;

namespace Shared.Security.Authorization.Permissions.Store;

/// <summary>Queries user and role permissions from the database by merging UserClaim and RoleClaim tables.</summary>
// Invariant: Permissions are stored as ClaimType=PermissionMetadataConstant.ClaimType claims; all queries return case-insensitive distinct sets.
// Context: Store always returns empty set on failure to prevent cascading authorization failures (Threat TMT-AUTH-001).
// Boundary: Store → Persistence — pure data access; no business logic or cache orchestration.
public sealed partial class PermissionStoreService(
    IApplicationDbContext dbContext,
    ILogger<PermissionStoreService> logger) : IPermissionStore
{
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<PermissionStoreService> _logger = logger;

    /// <summary>Returns all permissions for a user by merging role-based claims and direct user claims.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess && return.Value!=null, throws=never
    public async Task<Result<HashSet<string>>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Call: fetch claims from all roles assigned to the user via UserRole-RoleClaim join
            List<string?> roleClaims = await _dbContext.Set<UserRole>()
                .Where(ur => ur.UserId == userId)
                .Join(
                    _dbContext.Set<RoleClaim>(),
                    ur => ur.RoleId,
                    rc => rc.RoleId,
                    (ur, rc) => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Call: fetch claims assigned directly to the user account (not role-derived)
            List<string?> userClaims = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Transform: merge role and user claims into single case-insensitive unique set
            var allPermissions = roleClaims
                .Concat(userClaims)
                .Where(p => p != null)
                .Select(p => p!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: record quantity of permissions loaded for the user
            Loggers.LogPermissionsLoaded(_logger, allPermissions.Count, userId);

            return Result<HashSet<string>>.Ok(allPermissions, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Catch: log query failure and return empty set — prevents cascading authorization failure
            Loggers.LogGetPermissionsFailed(_logger, userId, ex);
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <summary>Returns all permissions assigned directly to a role via RoleClaim records.</summary>
    // Contract: pre=roleId!=Guid.Empty, post=return.IsSuccess && return.Value!=null, throws=never
    public async Task<Result<HashSet<string>>> GetRolePermissionsAsync(
        Guid roleId,
        CancellationToken ct = default)
    {
        try
        {
            // Call: fetch distinct claim values for the role (module boundary: Store → Persistence)
            List<string?> roleClaims = await _dbContext.Set<RoleClaim>()
                .Where(rc => rc.RoleId == roleId)
                .Select(rc => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Transform: filter nulls and build case-insensitive set
            var permissions = roleClaims
                .Where(p => p != null)
                .Select(p => p!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: record quantity of permissions loaded for the role
            Loggers.LogRolePermissionsLoaded(_logger, permissions.Count, roleId);

            return Result<HashSet<string>>.Ok(permissions, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Catch: log query failure and return empty set
            Loggers.LogGetRolePermissionsFailed(_logger, roleId, ex);
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <summary>Returns the set of role IDs assigned to a user.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess, throws=never
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
            // Catch: log and return empty set
            Loggers.LogGetPermissionsFailed(_logger, userId, ex);
            return Result<HashSet<Guid>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <summary>Returns only the direct (non-role) permissions assigned to a user via UserClaim records.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess && return.Value!=null, throws=never
    public async Task<Result<HashSet<string>>> GetUserDirectPermissionsAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Call: fetch claims assigned directly to the user account
            List<string?> userClaims = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Transform: filter nulls and build case-insensitive set
            var permissions = userClaims
                .Where(p => p != null)
                .Select(p => p!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: record quantity of direct permissions loaded
            Loggers.LogPermissionsLoaded(_logger, permissions.Count, userId);

            return Result<HashSet<string>>.Ok(permissions, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Catch: log query failure and return empty set
            Loggers.LogGetPermissionsFailed(_logger, userId, ex);
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }

    /// <summary>Adds permissions to a role by creating RoleClaim records.</summary>
    // Contract: pre=roleId!=Guid.Empty && permissions!=null, post=return.IsSuccess if committed, throws=never
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

    /// <summary>Removes permissions from a role by deleting matching RoleClaim records.</summary>
    // Contract: pre=roleId!=Guid.Empty && permissions!=null, post=return.IsSuccess if committed, throws=never
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

    /// <summary>Adds direct permissions to a user by creating UserClaim records.</summary>
    // Contract: pre=userId!=Guid.Empty && permissions!=null, post=return.IsSuccess if committed, throws=never
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

    /// <summary>Removes direct permissions from a user by deleting matching UserClaim records.</summary>
    // Contract: pre=userId!=Guid.Empty && permissions!=null, post=return.IsSuccess if committed, throws=never
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

    /// <summary>Loads all distinct permission identifiers across role and user claim tables.</summary>
    // Contract: post=return.IsSuccess && return.Value!=null, throws=never
    public async Task<Result<HashSet<string>>> GetAllPermissionIdentifiersAsync(CancellationToken ct = default)
    {
        try
        {
            // Call: query distinct permission claim values from role claims
            List<string?> roleIdentifiers = await _dbContext.Set<RoleClaim>()
                .Where(rc => rc.ClaimType == PermissionMetadataConstant.ClaimType)
                .Select(rc => rc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Call: query distinct permission claim values from user claims
            List<string?> userIdentifiers = await _dbContext.Set<UserClaim>()
                .Where(uc => uc.ClaimType == PermissionMetadataConstant.ClaimType)
                .Select(uc => uc.ClaimValue)
                .Distinct()
                .ToListAsync(ct);

            // Merge: combine role and user identifiers into single unique case-insensitive set
            var identifiers = roleIdentifiers
                .Concat(userIdentifiers)
                .Where(v => v != null)
                .Select(v => v!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Log: record total distinct permission identifiers loaded from store
            Loggers.LogAllIdentifiersLoaded(_logger, identifiers.Count);

            return Result<HashSet<string>>.Ok(identifiers, PermissionStoreResult.Success.Retrieved);
        }
        catch (Exception ex)
        {
            // Catch: return empty set on failure to prevent cascading errors
            Loggers.LogGetPermissionsFailed(_logger, Guid.Empty, ex);
            return Result<HashSet<string>>.Ok([], PermissionStoreResult.Success.Retrieved);
        }
    }
}
