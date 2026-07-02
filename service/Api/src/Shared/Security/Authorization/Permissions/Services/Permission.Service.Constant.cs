namespace Shared.Security.Authorization.Permissions.Services;

// Context: Constraints and error code patterns for permission service operations.
public static class PermissionServiceConstant
{
    public static class Constraints
    {
        public const int MaxPermissionsPerBatch = 100;
    }

    public static class Patterns
    {
        public const string ErrorCodePrefix = "PermissionService.";
    }
}
