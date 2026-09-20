

namespace Shared.Security.Authorization.Permissions.Services;

// Context: Result message constants for PermissionService operations — see PermissionService for usage.
public static class PermissionServiceResult
{
    public static class Success
    {
        public const string Resolved = "User effective permissions resolved.";
        public const string RoleResolved = "Role permissions resolved.";
        public const string Invalidated = "Permissions invalidated successfully.";
        public const string Added = "Permissions added successfully.";
        public const string Removed = "Permissions removed successfully.";
    }

    public static class Failure
    {
        public static Error Unexpected(string code, string message) => Error.Unexpected(code, message);
    }
}
