using Microsoft.Extensions.Options;

using Shared.Performance.Caching.Wrappers;
using Shared.Security.Authorization.Options;

namespace Shared.Security.Authorization.Permissions.Caches;

// Contract: pre=cacheService!=null && authzOptions!=null && logger!=null
public sealed partial class PermissionCache(
    ICacheService cacheService,
    IOptions<AuthzSetting> authzOptions,
    ILogger<PermissionCache> logger) : IPermissionCache
{
    private PermissionCacheOptions CacheOptions => authzOptions.Value.PermissionCache;

    // Compute: Build CachingEntryOption from configured PermissionCacheOptions TTL values.
    private CachingEntryOption CreateEntryOptions()
    {
        return new CachingEntryOption
        {
            Expiration = CacheOptions.AbsoluteExpiration, LocalCacheExpiration = CacheOptions.SlidingExpiration,
        };
    }

    /// <inheritdoc />
    // Contract: pre=userId!=Guid.Empty, post=return.IsSuccess
    public async Task<Result<HashSet<string>?>> GetAsync(Guid userId, CancellationToken ct = default)
    {
        // Cache: Probe user permission cache with key perm:user:{userId}.
        var key = $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}";

        // Guard: Return cached result if present; factory returns null for cache miss fallback.
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

    /// <inheritdoc />
    // Contract: pre=userId!=Guid.Empty && permissions!=null
    public async Task<Result> SetUserAsync(Guid userId, HashSet<string> permissions, IEnumerable<Guid>? roleIds = null,
        CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}";

        // Compute: Build tag list including user-scoped tag, global tag, and role-scoped invalidation tags.
        var tags = new List<string>(capacity: 2)
        {
            $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}", PermissionCacheConstant.Patterns.GlobalTag,
        };

        if (roleIds is not null)
        {
            tags.AddRange(roleIds.Select(id => $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{id}"));
        }

        CachingEntryOption options = CreateEntryOptions();

        // Cache: Persist resolved permission set with TTL and invalidation tags.
        await cacheService.SetAsync(key, permissions, options, tags, ct);

        // Log: Record cache write with configured sliding expiration duration.
        Loggers.LogPermissionsCached(logger, userId, CacheOptions.SlidingExpiration);
        return Result.Ok(PermissionCacheResult.Success.Cached);
    }

    /// <inheritdoc />
    // Contract: pre=userId!=Guid.Empty
    public async Task<Result> InvalidateUserAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}";

        // Cache: Remove user-specific cache entry by key.
        await cacheService.RemoveAsync(key, ct);

        // Cache: Purge all entries tagged with this user ID.
        await cacheService.RemoveByTagAsync($"{PermissionCacheConstant.Patterns.UserKeyPrefix}{userId}", ct);

        Loggers.LogCacheInvalidated(logger, userId);
        return Result.Ok(PermissionCacheResult.Success.Invalidated);
    }

    /// <inheritdoc />
    // Cache: Purge all permission cache entries via global tag.
    public async Task<Result> InvalidateAllAsync(CancellationToken ct = default)
    {
        await cacheService.RemoveByTagAsync(PermissionCacheConstant.Patterns.GlobalTag, ct);

        Loggers.LogAllPermissionsInvalidated(logger);
        return Result.Ok(PermissionCacheResult.Success.AllInvalidated);
    }

    /// <inheritdoc />
    // Contract: pre=roleId!=Guid.Empty, post=return.IsSuccess
    public async Task<Result<HashSet<string>?>> GetRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        // Cache: Probe role permission cache with key perm:role:{roleId}.
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

    /// <inheritdoc />
    // Contract: pre=roleId!=Guid.Empty && permissions!=null
    public async Task<Result> SetRoleAsync(Guid roleId, HashSet<string> permissions, CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}";

        // Compute: Build tag list including role-scoped tag and global tag.
        var tags = new List<string>(capacity: 2)
        {
            $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}", PermissionCacheConstant.Patterns.GlobalTag,
        };

        CachingEntryOption options = CreateEntryOptions();

        // Cache: Persist resolved role permission set with TTL and invalidation tags.
        await cacheService.SetAsync(key, permissions, options, tags, ct);

        Loggers.LogRolePermissionsCached(logger, roleId, CacheOptions.SlidingExpiration);
        return Result.Ok(PermissionCacheResult.Success.Cached);
    }

    /// <inheritdoc />
    // Contract: pre=roleId!=Guid.Empty
    public async Task<Result> InvalidateRoleAsync(Guid roleId, CancellationToken ct = default)
    {
        var key = $"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}";

        // Cache: Remove role-specific cache entry by key.
        await cacheService.RemoveAsync(key, ct);

        // Cache: Purge all user cache entries tagged with this role ID.
        await cacheService.RemoveByTagAsync($"{PermissionCacheConstant.Patterns.RoleKeyPrefix}{roleId}", ct);

        Loggers.LogRoleCacheInvalidated(logger, roleId);
        return Result.Ok(PermissionCacheResult.Success.RoleInvalidated);
    }
}