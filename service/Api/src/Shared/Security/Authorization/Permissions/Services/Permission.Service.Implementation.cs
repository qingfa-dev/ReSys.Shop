using Shared.Security.Authorization.Permissions.Caches;
using Shared.Security.Authorization.Permissions.Store;

namespace Shared.Security.Authorization.Permissions.Services;

// Contract: pre=cache!=null && store!=null && logger!=null
public sealed partial class PermissionService(
    IPermissionCache cache,
    IPermissionStore store,
    ILogger<PermissionService> logger)
    : IPermissionService
{
    /// <inheritdoc />
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess
    public async Task<Result<HashSet<string>>> GetEffectiveUserPermissionsAsync(Guid userId,
        CancellationToken ct = default)
    {
        try
        {
            // Check: Probe user cache for pre-resolved permission set.
            Result<HashSet<string>?> cacheResult = await cache.GetAsync(userId, ct);

            // Guard: Return cached set immediately if found.
            if (cacheResult is { IsSuccess: true, Value: not null })
            {
                Loggers.LogEffectivePermissionsResolved(logger, cacheResult.Value.Count, userId);
                return Result<HashSet<string>>.Ok(cacheResult.Value, PermissionServiceResult.Success.Resolved);
            }

            // Receive: Fetch role IDs assigned to the user from the store.
            Result<HashSet<Guid>> rolesResult = await store.GetUserRoleIdsAsync(userId, ct);
            HashSet<Guid> roleIds = rolesResult.IsSuccess ? rolesResult.Value : [];

            // Batch: Resolve permissions for each role in parallel via cache-aware pipeline.
            IEnumerable<Task<HashSet<string>>> rolePermissionTasks = roleIds.Select(async roleId =>
            {
                Result<HashSet<string>?> roleCacheResult = await cache.GetRoleAsync(roleId, ct);
                if (roleCacheResult is { IsSuccess: true, Value: not null })
                    return roleCacheResult.Value;

                // Cache: Fallback to store on cache miss; populate role cache after resolution.
                Result<HashSet<string>> roleStoreResult = await store.GetRolePermissionsAsync(roleId, ct);
                HashSet<string> rolePerms = roleStoreResult.IsSuccess ? roleStoreResult.Value : [];

                if (roleStoreResult.IsSuccess)
                    await cache.SetRoleAsync(roleId, rolePerms, ct);

                return rolePerms;
            });

            // Receive: Fetch direct user permissions (non-role claims) from store.
            Task<Result<HashSet<string>>> userDirectPermsTask = store.GetUserDirectPermissionsAsync(userId, ct);

            // Await: Execute all role resolution tasks concurrently.
            HashSet<string>[] rolePermissionSets = await Task.WhenAll(rolePermissionTasks);
            Result<HashSet<string>> userDirectResult = await userDirectPermsTask;
            HashSet<string> userDirectPerms = userDirectResult.IsSuccess ? userDirectResult.Value : [];

            // Merge: Union all role permission sets with direct user permissions.
            var permissions = new HashSet<string>(userDirectPerms, StringComparer.OrdinalIgnoreCase);
            foreach (HashSet<string>? set in rolePermissionSets)
                permissions.UnionWith(set);

            // Cache: Populate user cache with role tags for targeted invalidation.
            await cache.SetUserAsync(userId, permissions, roleIds, ct);

            Loggers.LogEffectivePermissionsResolved(logger, permissions.Count, userId);
            return Result<HashSet<string>>.Ok(permissions, PermissionServiceResult.Success.Resolved);
        }
        catch (Exception ex)
        {
            // Catch: Return empty set on unexpected failure to prevent cascading errors.
            Loggers.LogUserResolutionFailed(logger, userId, ex.Message);

            // Fallback: Always return success with empty set — never throw in auth pipeline.
            return Result<HashSet<string>>.Ok(new HashSet<string>(), PermissionServiceResult.Success.Resolved);
        }
    }

    /// <inheritdoc />
    // Contract: pre=roleId!=Guid.Empty
    public async Task<Result<HashSet<string>>> GetRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        try
        {
            // Check: Probe role cache first.
            Result<HashSet<string>?> roleCacheResult = await cache.GetRoleAsync(roleId, ct);
            if (roleCacheResult.IsSuccess && roleCacheResult.Value != null)
            {
                Loggers.LogRolePermissionsResolved(logger, roleCacheResult.Value.Count, roleId);
                return Result<HashSet<string>>.Ok(roleCacheResult.Value, PermissionServiceResult.Success.RoleResolved);
            }

            // Fallback: Query store on cache miss.
            Result<HashSet<string>> roleStoreResult = await store.GetRolePermissionsAsync(roleId, ct);
            HashSet<string> rolePerms = roleStoreResult.IsSuccess ? roleStoreResult.Value : [];

            // Cache: Populate role cache for subsequent requests.
            if (roleStoreResult.IsSuccess)
                await cache.SetRoleAsync(roleId, rolePerms, ct);

            Loggers.LogRolePermissionsResolved(logger, rolePerms.Count, roleId);
            return Result<HashSet<string>>.Ok(rolePerms, PermissionServiceResult.Success.RoleResolved);
        }
        catch (Exception ex)
        {
            // Catch: Return empty set on failure.
            Loggers.LogRoleResolutionFailed(logger, roleId, ex.Message);
            return Result<HashSet<string>>.Ok(new HashSet<string>(), PermissionServiceResult.Success.RoleResolved);
        }
    }

    /// <inheritdoc />
    // Contract: pre=userId!=Guid.Empty && permissions!=null
    public async Task<Result<bool>> HasAllPermissionsAsync(Guid userId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        // Verify: Check that every specified permission exists in the user's effective set.
        Result<HashSet<string>> userPermsResult = await GetEffectiveUserPermissionsAsync(userId, ct);

        // Guard: Propagate failure early if permission resolution failed.
        if (userPermsResult.IsFailure) return userPermsResult.Errors;

        var hasAll = permissions.All(p => userPermsResult.Value.Contains(p));
        return hasAll;
    }

    /// <inheritdoc />
    // Contract: pre=roleId!=Guid.Empty && permissions!=null
    public async Task<Result<bool>> RoleHasAllPermissionsAsync(Guid roleId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        // Verify: Check that every specified permission exists in the role's permission set.
        Result<HashSet<string>> rolePermsResult = await GetRolePermissionsAsync(roleId, ct);
        if (rolePermsResult.IsFailure) return rolePermsResult.Errors;

        var hasAll = permissions.All(p => rolePermsResult.Value.Contains(p));
        return hasAll;
    }

    /// <inheritdoc />
    // Delegate: Forward invalidation to cache layer for cascade (role + tagged users).
    public Task<Result> InvalidateRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        return cache.InvalidateRoleAsync(roleId, ct);
    }

    /// <inheritdoc />
    // Delegate: Forward invalidation to cache layer for single user.
    public Task<Result> InvalidateUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        return cache.InvalidateUserAsync(userId, ct);
    }

    /// <inheritdoc />
    // Batch: Persist new permissions to store, then invalidate cache for consistency.
    public async Task<Result> AddRolePermissionsAsync(Guid roleId, IEnumerable<string> permissions,
        CancellationToken ct = default)
    {
        Result addResult = await store.AddRolePermissionsAsync(roleId, permissions, ct);

        if (addResult.IsSuccess)
        {
            // Cache: Invalidate role + cascading user caches after successful store mutation.
            await cache.InvalidateRoleAsync(roleId, ct);
            return Result.Ok(PermissionServiceResult.Success.Added);
        }

        return addResult;
    }

    /// <inheritdoc />
    // Batch: Remove permissions from store, then invalidate cache for consistency.
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

    /// <inheritdoc />
    // Batch: Persist direct user permissions to store, then invalidate user cache.
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

    /// <inheritdoc />
    // Batch: Remove direct user permissions from store, then invalidate user cache.
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
