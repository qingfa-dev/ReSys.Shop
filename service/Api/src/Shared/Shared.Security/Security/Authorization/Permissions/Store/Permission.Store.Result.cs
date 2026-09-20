namespace Shared.Security.Authorization.Permissions.Store;

// Context: Result message constants for PermissionStore operations — see PermissionStoreService for usage.
public static class PermissionStoreResult
{
    public static class Success
    {
        public const string Retrieved = "Permissions retrieved successfully.";
    }

    public static class Failure
    {
        public static Error Unexpected(string code, string message) => Error.Unexpected(code, message);
    }
}
