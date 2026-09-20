namespace Shared.Security.Authorization.Permissions.Caches;

// Boundary: Caching → Domain — Wraps ICacheService with permission-domain semantics (key prefixes, role tags, configurable TTL).

/// <summary>
/// Caches per-user permission sets to prevent a DB round-trip on every request.
///
/// Cache lifetime:    Sliding 5-minute TTL (configurable).
/// Invalidation:      Explicit call to InvalidateAsync() after any role/claim change.
///                    This is the key advantage over JWT baking — permission
///                    changes take effect on the NEXT request after invalidation,
///                    not at the next login.
///
/// In production at scale, swap IMemoryCache for IDistributedCache (Redis) so
/// all pods share a single cache and invalidation propagates cluster-wide.
/// </summary>
public interface IPermissionCache
{
    /// <summary>
    /// Retrieves cached permissions for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the cached permissions or null if not found.</returns>
    Task<Result<HashSet<string>?>> GetAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Stores permissions in the cache for a user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="permissions">The set of permission strings to cache.</param>
    /// <param name="roleIds">Optional set of role IDs assigned to the user for role-based invalidation tagging.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> SetUserAsync(Guid userId, HashSet<string> permissions, IEnumerable<Guid>? roleIds = null, CancellationToken ct = default);

    /// <summary>
    /// Invalidates cached permissions for a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> InvalidateUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Global invalidation: Purges all cached permissions across all users.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> InvalidateAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a set of permissions for a role from the distributed cache.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result containing the permission set or null if a cache miss occurs.</returns>
    Task<Result<HashSet<string>?>> GetRoleAsync(Guid roleId, CancellationToken ct = default);

    /// <summary>
    /// Populates the distributed cache with a set of permissions for a role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role.</param>
    /// <param name="permissions">The set of permissions to cache.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> SetRoleAsync(Guid roleId, HashSet<string> permissions, CancellationToken ct = default);

    /// <summary>
    /// Invalidate by Role: Purges specific role entry across all cache tiers using tags.
    /// Also invalidates any user entries tagged with this role.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to invalidate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> InvalidateRoleAsync(Guid roleId, CancellationToken ct = default);
}

