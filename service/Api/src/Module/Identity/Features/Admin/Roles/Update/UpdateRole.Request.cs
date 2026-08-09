using Module.Identity.Features.Shared.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Update;

public static partial class UpdateRole
{
    /// <summary>
    /// Represents the request contract for updating an existing role.
    /// Inherits common role properties from <see cref="RoleRequest"/>.
    /// </summary>
    public record Request : RoleRequest;
}