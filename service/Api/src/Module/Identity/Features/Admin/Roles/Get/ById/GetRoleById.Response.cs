using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Get.ById;

public static partial class GetRoleById
{
    /// <summary>
    /// Represents the response contract for retrieving a role by its ID.
    /// Inherits properties like Id, Name, Description, IsSystem, and audit fields from <see cref="RoleDetailResponse"/>.
    /// </summary>
    public class Response : RoleDetailResponse { }
}
