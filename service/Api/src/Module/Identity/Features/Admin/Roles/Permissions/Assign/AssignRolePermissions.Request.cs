using Module.Identity.Features.Shared.Admin.Permissions.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Permissions.Assign;

public static partial class AssignRolePermissions
{
    /// <summary>
    /// Represents the request contract for assigning permissions to a role.
    /// </summary>
    public record Request : PermissionCollectionParameters;
}