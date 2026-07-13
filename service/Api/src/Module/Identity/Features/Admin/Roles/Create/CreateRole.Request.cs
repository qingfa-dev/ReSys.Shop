using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Create;

public static partial class CreateRole
{
    /// <summary>
    /// Represents the request contract for creating a new role.
    /// Inherits properties like Name and Description from <see cref="RoleRequest"/>.
    /// </summary>
    public class Request : RoleRequest { }
}