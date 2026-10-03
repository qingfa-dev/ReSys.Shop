namespace Shared.Security.Authorization.Permissions.Services;

public partial class PermissionService
{
    /// <summary>Structured log event definitions for PermissionService operations.</summary>
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 144,
            Level = LogLevel.Debug,
            Message = "PermissionService: resolved {Count} effective permissions for user {UserId}")]
        public static partial void LogEffectivePermissionsResolved(ILogger logger, int count, Guid userId);

        [LoggerMessage(
            EventId = 145,
            Level = LogLevel.Debug,
            Message = "PermissionService: resolved {Count} permissions for role {RoleId}")]
        public static partial void LogRolePermissionsResolved(ILogger logger, int count, Guid roleId);

        [LoggerMessage(
            EventId = 146,
            Level = LogLevel.Error,
            Message = "PermissionService: unexpected failure while resolving permissions for user {UserId}: {Error}")]
        public static partial void LogUserResolutionFailed(ILogger logger, Guid userId, string? error);

        [LoggerMessage(
            EventId = 147,
            Level = LogLevel.Error,
            Message = "PermissionService: unexpected failure while resolving permissions for role {RoleId}: {Error}")]
        public static partial void LogRoleResolutionFailed(ILogger logger, Guid roleId, string? error);
    }
}
