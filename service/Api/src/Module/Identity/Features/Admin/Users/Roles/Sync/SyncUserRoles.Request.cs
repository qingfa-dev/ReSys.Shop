using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Sync;

public static partial class SyncUserRoles
{
    /// <summary>
    /// Represents the request contract for synchronizing a user's roles.
    /// This will replace the user's current roles with the specified list.
    /// </summary>
    public record Request : RoleCollectionParameters;
}