using Shared.Security.Authorization.Permissions.Caches;
using Shared.Security.Authorization.Permissions.Store;

namespace Shared.Security.Authorization.Permissions.Services;

/// <summary>Resolves effective user permissions by merging role-based and direct permissions through a cache-aside pattern with parallel role resolution.</summary>
// Invariant: Effective permissions are the union of role permissions + direct user permissions; cache is invalidated on every store mutation.
// Context: Permission resolution must never throw in the authorization pipeline — empty set is the safe default (Threat TMT-AUTH-001).
// Boundary: Service → Cache | Store — orchestrates between two data sources; never calls the database directly.
// Contract: pre=cache!=null && store!=null && logger!=null
public sealed partial class PermissionService(
    IPermissionCache cache,
    IPermissionStore store,
    ILogger<PermissionService> logger)
    : IPermissionService
{
    /// <summary>Resolves effective permissions for a user by merging role + direct permissions with cache-aside and parallel role resolution.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess && return.Value!=null, throws=never — returns empty set on failure
    public async Task<Result<HashSet<string>>> GetEffectiveUserPermissionsAsync(Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Cache: probe user cache for pre-resolved permission set (module boundary: Service → Cache)
            Result<HashSet<string>?> cacheResult = await cache.GetAsync(userId, ct);

            // Guard: return cached set immediately if found — avoids store round-trips
            if (cacheResult is { IsSuccess: true, Value: not null })
            {
                Loggers.LogEffectivePermissionsResolved(logger, cacheResult.Value.Count, userId);
                return Result<HashSet<string>>.Ok(cacheResult.Value, PermissionServiceResult.Success.Resolved);
            }

            // Call: fetch role IDs assigned to user from store (module boundary: Service → Store)
            Result<HashSet<Guid>> rolesResult = await store.GetUserRoleIdsAsync(userId, ct);
            HashSet<Guid> roleIds = rolesResult.IsSuccess ? rolesResult.Value : [];

            // Call: fetch direct user permissions (non-role claims) from store
            Result<HashSet<string>> userDirectResult = await store.GetUserDirectPermissionsAsync(userId, ct);
            HashSet<string> userDirectPerms = userDirectResult.IsSuccess ? userDirectResult.Value : [];

            // Resolve: resolve permissions for each role sequentially — shared DbContext cannot parallelize
            var rolePermissionSets = new List<HashSet<string>>(roleIds.Count);
            foreach (Guid roleId in roleIds)
            {
                Result<HashSet<string>?> roleCacheResult = await cache.GetRoleAsync(roleId, ct);
                if (roleCacheResult is { IsSuccess: true, Value: not null })
                {
                    rolePermissionSets.Add(roleCacheResult.Value);
                    continue;
                }

                Result<HashSet<string>> roleStoreResult = await store.GetRolePermissionsAsync(roleId, ct);
                HashSet<string> rolePerms = roleStoreResult.IsSuccess ? roleStoreResult.Value : [];

                if (roleStoreResult.IsSuccess)
                    await cache.SetRoleAsync(roleId, rolePerms, ct);

                rolePermissionSets.Add(rolePerms);
            }

            // Merge: union all role permission sets with direct user permissions — case-insensitive dedup
            var permissions = new HashSet<string>(userDirectPerms, StringComparer.OrdinalIgnoreCase);
            foreach (HashSet<string>? set in rolePermissionSets)
                permissions.UnionWith(set);

            // Cache: populate user cache with role tags for targeted invalidation on role change
            await cache.SetUserAsync(userId, permissions, roleIds, ct);

            Loggers.LogEffectivePermissionsResolved(logger, permissions.Count, userId);
            return Result<HashSet<string>>.Ok(permissions, PermissionServiceResult.Success.Resolved);
        }
        catch (Exception ex)
        {
            // Catch: return empty set on unexpected failure — never throw in auth pipeline per TMT-AUTH-001
            Loggers.LogUserResolutionFailed(logger, userId, ex.Message);

            // Fallback: always return success with empty set to prevent cascading authorization failures
            return Result<HashSet<string>>.Ok(new HashSet<string>(), PermissionServiceResult.Success.Resolved);
        }
    }

    /// <summary>Resolves permissions for a role using cache-aside pattern.</summary>
    // Contract: pre=roleId!=Guid.Empty, post=return.IsSuccess && return.Value!=null, throws=never
    public async Task<Result<HashSet<string>>> GetRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        try
        {
            // Cache: probe role cache first (module boundary: Service → Cache)
            Result<HashSet<string>?> roleCacheResult = await cache.GetRoleAsync(roleId, ct);
            if (roleCacheResult.IsSuccess && roleCacheResult.Value != null)
            {
                Loggers.LogRolePermissionsResolved(logger, roleCacheResult.Value.Count, roleId);
                return Result<HashSet<string>>.Ok(roleCacheResult.Value, PermissionServiceResult.Success.RoleResolved);
            }

            // Call: query store on cache miss (module boundary: Service → Store)
            Result<HashSet<string>> roleStoreResult = await store.GetRolePermissionsAsync(roleId, ct);
            HashSet<string> rolePerms = roleStoreResult.IsSuccess ? roleStoreResult.Value : [];

            // Cache: populate role cache for subsequent requests
            if (roleStoreResult.IsSuccess)
                await cache.SetRoleAsync(roleId, rolePerms, ct);

            Loggers.LogRolePermissionsResolved(logger, rolePerms.Count, roleId);
            return Result<HashSet<string>>.Ok(rolePerms, PermissionServiceResult.Success.RoleResolved);
        }
        catch (Exception ex)
        {
            // Catch: return empty set on failure — prevents auth pipeline crash
            Loggers.LogRoleResolutionFailed(logger, roleId, ex.Message);
            return Result<HashSet<string>>.Ok(new HashSet<string>(), PermissionServiceResult.Success.RoleResolved);
        }
    }

    /// <summary>Verifies that a user has ALL specified permissions.</summary>
    // Contract: pre=userId!=Guid.Empty && permissions!=null, post=return.IsSuccess, throws=never
    public async Task<Result<bool>> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        // Call: resolve effective user permissions
        Result<HashSet<string>> userPermsResult = await GetEffectiveUserPermissionsAsync(userId, ct);

        // Guard: propagate failure early if permission resolution failed
        if (userPermsResult.IsFailure) return userPermsResult.Errors;

        var hasAll = permissions.All(p => userPermsResult.Value.Contains(p));
        return hasAll;
    }

    /// <summary>Verifies that a role has ALL specified permissions.</summary>
    // Contract: pre=roleId!=Guid.Empty && permissions!=null, post=return.IsSuccess, throws=never
    public async Task<Result<bool>> RoleHasAllPermissionsAsync(Guid roleId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        // Call: resolve role permissions
        Result<HashSet<string>> rolePermsResult = await GetRolePermissionsAsync(roleId, ct);
        if (rolePermsResult.IsFailure) return rolePermsResult.Errors;

        var hasAll = permissions.All(p => rolePermsResult.Value.Contains(p));
        return hasAll;
    }

    /// <summary>Invalidates cached permissions for a role and all users tagged with that role.</summary>
    // Delegate: forward invalidation to cache layer for cascade (role + tagged users)
    public Task<Result> InvalidateRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        return cache.InvalidateRoleAsync(roleId, ct);
    }

    /// <summary>Invalidates cached permissions for a single user.</summary>
    // Delegate: forward invalidation to cache layer for single user
    public Task<Result> InvalidateUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        return cache.InvalidateUserAsync(userId, ct);
    }

    /// <summary>Persists new permissions to a role and invalidates cache for consistency.</summary>
    // Contract: pre=roleId!=Guid.Empty && permissions!=null, post=return.IsSuccess if store succeeds, throws=never
    public async Task<Result> AddRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        Result addResult = await store.AddRolePermissionsAsync(roleId, permissions, ct);

        if (addResult.IsSuccess)
        {
            // Cache: invalidate role + cascading user caches after successful store mutation
            await cache.InvalidateRoleAsync(roleId, ct);
            return Result.Ok(PermissionServiceResult.Success.Added);
        }

        return addResult;
    }

    /// <summary>Removes permissions from a role and invalidates cache for consistency.</summary>
    // Contract: pre=roleId!=Guid.Empty && permissions!=null, post=return.IsSuccess if store succeeds, throws=never
    public async Task<Result> RemoveRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        Result removeResult = await store.RemoveRolePermissionsAsync(roleId, permissions, ct);

        if (removeResult.IsSuccess)
        {
            await cache.InvalidateRoleAsync(roleId, ct);
            return Result.Ok(PermissionServiceResult.Success.Removed);
        }

        return removeResult;
    }

    /// <summary>Persists direct permissions to a user and invalidates user cache.</summary>
    // Contract: pre=userId!=Guid.Empty && permissions!=null, post=return.IsSuccess if store succeeds, throws=never
    public async Task<Result> AddUserDirectPermissionsAsync(Guid userId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        Result addResult = await store.AddUserDirectPermissionsAsync(userId, permissions, ct);

        if (addResult.IsSuccess)
        {
            await cache.InvalidateUserAsync(userId, ct);
            return Result.Ok(PermissionServiceResult.Success.Added);
        }

        return addResult;
    }

    /// <summary>Removes direct permissions from a user and invalidates user cache.</summary>
    // Contract: pre=userId!=Guid.Empty && permissions!=null, post=return.IsSuccess if store succeeds, throws=never
    public async Task<Result> RemoveUserDirectPermissionsAsync(Guid userId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        Result removeResult = await store.RemoveUserDirectPermissionsAsync(userId, permissions, ct);

        if (removeResult.IsSuccess)
        {
            await cache.InvalidateUserAsync(userId, ct);
            return Result.Ok(PermissionServiceResult.Success.Removed);
        }

        return removeResult;
    }
}
