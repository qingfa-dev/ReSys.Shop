using Module.Identity.Features.Shared.Admin.Users.Roles.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    public record Request : RoleCollectionParameters;
}