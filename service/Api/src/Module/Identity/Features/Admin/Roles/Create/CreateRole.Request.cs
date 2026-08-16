using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Create;

public static partial class CreateRole
{
    /// <summary>
    /// Represents the request contract for creating a new role.
    /// Inherits properties like Name and Description from <see cref="RoleRequest"/>.
    /// </summary>
    public record Request : RoleRequest;
}