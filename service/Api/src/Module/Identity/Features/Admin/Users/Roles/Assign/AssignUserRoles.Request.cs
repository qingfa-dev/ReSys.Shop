using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Assign;

public static partial class AssignUserRoles
{
    public record Request : RoleCollectionParameters;
}