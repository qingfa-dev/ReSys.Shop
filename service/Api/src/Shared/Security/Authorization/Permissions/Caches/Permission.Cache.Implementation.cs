using Microsoft.Extensions.Options;

using Shared.Performance.Caching.Wrappers;
using Shared.Security.Authorization.Options;

namespace Shared.Security.Authorization.Permissions.Caches;

/// <summary>Two-tier permission cache (local + distributed) with role-tagged invalidation and configurable TTL.</summary>
// Invariant: Cache keys follow perm:user|role:{id} pattern; role invalidation cascades to all users tagged with that role.
// Boundary: Cache → CacheService — delegates to ICacheService wrapper; never accesses cache infrastructure directly.
// Contract: pre=cacheService!=null && authzOptions!=null && logger!=null
public sealed partial class PermissionCache(
    ICacheService cacheService,
    IOptions<AuthzSetting> authzOptions,
    ILogger<PermissionCache> logger) : IPermissionCache
{
    private PermissionCacheOptions CacheOptions => authzOptions.Value.PermissionCache;

    // Compute: build CachingEntryOption from configured PermissionCacheOptions TTL values
    private CachingEntryOption CreateEntryOptions()
    {
        return new CachingEntryOption
        {
            Expiration = CacheOptions.AbsoluteExpiration, LocalCacheExpiration = CacheOptions.SlidingExpiration,
        };
    }

    /// <summary>Retrieves cached user permissions — returns null on cache miss for caller to fall back to store.</summary>
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess, throws=never
    public async Task<Result<HashSet<string>?>> GetAsync(Guid userId, CancellationToken ct = default)
    {
        // Cache: probe user permission cache with key perm:user:{userId} (module boundary: Cache → CacheService)
        var key = $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}";

        // Cache: factory returns null for cache miss — caller resolves from store and repopulates
        HashSet<string>? result = await cacheService.GetOrCreateAsync<HashSet<string>?>(
            key,
            _ => ValueTask.FromResult<HashSet<string>?>(null),
            cancellationToken: ct);

        if (result is not null)
        {
            Loggers.LogCacheHit(logger, userId);
        }
        else
        {
            Loggers.LogCacheMiss(logger, userId);
        }

        return Result<HashSet<string>?>.Ok(result, result is not null ? PermissionCacheResult.Success.Retrieved : null);
    }

    /// <summary>Persists user permissions with role-tagged invalidation tags for cascade eviction.</summary>
    // Contract: pre=userId!=Guid.Empty && permissions!=null, post=cache entry created, throws=never
    public async Task<Result> SetUserAsync(Guid userId, HashSet<string> permissions, IEnumerable<Guid>? roleIds = null,
        CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}";

        // Compute: build tag list including user-scoped tag, global tag, and role-scoped invalidation tags
        var tags = new List<string>(capacity: 2)
        {
            $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}", PermissionCacheConstant.Patterns.GlobalTag,
        };

        if (roleIds is not null)
        {
            // Cache: role tags enable cascade invalidation when role permissions change
            tags.AddRange(roleIds.Select(id => $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{id}"));
        }

        CachingEntryOption options = CreateEntryOptions();

        // Cache: persist resolved permission set with TTL and invalidation tags (module boundary: Cache → CacheService)
        await cacheService.SetAsync(key, permissions, options, tags, ct);

        // Log: record cache write with configured sliding expiration duration
        Loggers.LogPermissionsCached(logger, userId, CacheOptions.SlidingExpiration);
        return Result.Ok(PermissionCacheResult.Success.Cached);
    }

    /// <summary>Removes user-specific cache entry and all tag-associated entries.</summary>
    // Contract: pre=userId!=Guid.Empty, post=user cache entries purged, throws=never
    public async Task<Result> InvalidateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}";

        // Cache: remove user-specific cache entry by key
        await cacheService.RemoveAsync(key, ct);

        // Cache: purge all entries tagged with this user ID for complete invalidation
        await cacheService.RemoveByTagAsync($"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}", ct);

        Loggers.LogCacheInvalidated(logger, userId);
        return Result.Ok(PermissionCacheResult.Success.Invalidated);
    }

    /// <summary>Purges ALL permission cache entries via global tag.</summary>
    // Contract: post=all permission cache entries purged, throws=never
    public async Task<Result> InvalidateAllAsync(CancellationToken ct = default)
    {
        await cacheService.RemoveByTagAsync(PermissionCacheConstant.Patterns.GlobalTag, ct);

        Loggers.LogAllPermissionsInvalidated(logger);
        return Result.Ok(PermissionCacheResult.Success.AllInvalidated);
    }

    /// <summary>Retrieves cached role permissions — returns null on cache miss for caller to fall back to store.</summary>
    // Contract: pre=roleId!=Guid.Empty, post=return.IsSuccess, throws=never
    public async Task<Result<HashSet<string>?>> GetRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        // Cache: probe role permission cache with key perm:role:{roleId} (module boundary: Cache → CacheService)
        var key = $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}";

        HashSet<string>? result = await cacheService.GetOrCreateAsync<HashSet<string>?>(
            key,
            _ => ValueTask.FromResult<HashSet<string>?>(null),
            cancellationToken: ct);

        if (result is not null)
        {
            Loggers.LogRoleCacheHit(logger, roleId);
        }
        else
        {
            Loggers.LogRoleCacheMiss(logger, roleId);
        }

        return Result<HashSet<string>?>.Ok(result);
    }

    /// <summary>Persists role permissions with role-scoped and global invalidation tags.</summary>
    // Contract: pre=roleId!=Guid.Empty && permissions!=null, post=cache entry created, throws=never
    public async Task<Result> SetRoleAsync(Guid roleId, HashSet<string> permissions, CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}";

        // Compute: build tag list including role-scoped tag and global tag for cascade invalidation
        var tags = new List<string>(capacity: 2)
        {
            $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}", PermissionCacheConstant.Patterns.GlobalTag,
        };

        CachingEntryOption options = CreateEntryOptions();

        // Cache: persist resolved role permission set with TTL and invalidation tags (module boundary: Cache → CacheService)
        await cacheService.SetAsync(key, permissions, options, tags, ct);

        Loggers.LogRolePermissionsCached(logger, roleId, CacheOptions.SlidingExpiration);
        return Result.Ok(PermissionCacheResult.Success.Cached);
    }

    /// <summary>Removes role-specific cache entry and purges all user entries tagged with this role.</summary>
    // Contract: pre=roleId!=Guid.Empty, post=role cache entries purged, throws=never
    public async Task<Result> InvalidateRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}";

        // Cache: remove role-specific cache entry by key
        await cacheService.RemoveAsync(key, ct);

        // Cache: purge all user cache entries tagged with this role ID for cascade invalidation
        await cacheService.RemoveByTagAsync($"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}", ct);

        Loggers.LogRoleCacheInvalidated(logger, roleId);
        return Result.Ok(PermissionCacheResult.Success.RoleInvalidated);
    }
}