using Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Sync;

public static partial class SyncRolePermissions
{
    /// <summary>
    /// Represents the request to synchronize all permissions for a specific role.
    /// </summary>
    public record Request : PermissionCollectionParameters;
}