namespace Shared.Security.Authorization.Permissions.Store;

// Context: Constraints and error code patterns for permission store operations.
public static class PermissionStoreConstant
{
    public static class Constraints
    {
        public const int MaxPermissionsPerBatch = 1000;
    }

    public static class Patterns
    {
        public const string ErrorCodePrefix = "PermissionStore.";
        public const string BatchAddFailed = "PermissionStore.BatchAddFailed";
        public const string BatchRemoveFailed = "PermissionStore.BatchRemoveFailed";
        public const string GetAllIdentifiersFailed = "PermissionStore.GetAllIdentifiers.Failed";
    }
}
