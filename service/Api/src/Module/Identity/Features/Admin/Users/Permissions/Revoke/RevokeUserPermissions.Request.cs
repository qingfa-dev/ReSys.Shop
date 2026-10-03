using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Permissions.Revoke;

public static partial class RevokeUserPermissions
{
    /// <summary>
    /// Represents the request contract for revoking direct permissions from a user.
    /// </summary>
    public record Request : PermissionCollectionParameters;
}