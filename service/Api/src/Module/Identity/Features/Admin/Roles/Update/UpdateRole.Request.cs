using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Update;

public static partial class UpdateRole
{
    /// <summary>
    /// Represents the request contract for updating an existing role.
    /// Inherits common role properties from <see cref="RoleRequest"/>.
    /// </summary>
    public class Request : RoleRequest;
}