

namespace Shared.Security.Authorization.Permissions.Caches;

// Context: Result message constants for PermissionCache operations — see PermissionCache for usage.
public static class PermissionCacheResult
{
    public static class Success
    {
        public const string Retrieved = "User permissions retrieved from cache.";
        public const string RoleRetrieved = "Role permissions retrieved from cache.";
        public const string Cached = "Permissions cached successfully.";
        public const string Invalidated = "User cache invalidated successfully.";
        public const string RoleInvalidated = "Role cache invalidated successfully.";
        public const string AllInvalidated = "All permission caches invalidated.";
    }

    public static class Failure
    {
        public static Error Unexpected(string code, string message) => Error.Unexpected(code, message);
    }
}
