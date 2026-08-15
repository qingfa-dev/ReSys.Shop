using Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Permissions.Sync;

public static partial class SyncUserPermissions
{
    /// <summary>
    /// Represents the request contract for synchronizing direct permissions for a user.
    /// This will replace the user's current direct permissions with the specified list.
    /// </summary>
    public record Request : PermissionCollectionParameters;
}