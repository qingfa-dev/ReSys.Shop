using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Revoke;

public static partial class RevokeUserRoles
{
    /// <summary>
    /// Represents the request contract for revoking roles from a user.
    /// </summary>
    public record Request : RoleCollectionParameters;
}