using Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Permissions.Assign;

public static partial class AssignUserPermissions
{
    /// <summary>
    /// Represents the request contract for assigning direct permissions to a user.
    /// </summary>
    public record Request : PermissionCollectionParameters;
}