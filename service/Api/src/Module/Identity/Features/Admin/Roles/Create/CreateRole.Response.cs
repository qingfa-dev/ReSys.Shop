using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Create;

public static partial class CreateRole
{
    /// <summary>
    /// Represents the response contract for a created role.
    /// Inherits properties like Id, Name, Description, IsSystem, and audit fields from <see cref="RoleDetailResponse"/>.
    /// </summary>
    public class Response : RoleDetailResponse { }
}