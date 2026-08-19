using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Get.ById;

public static partial class GetRoleById
{
    /// <summary>
    /// Represents the response contract for retrieving a role by its ID.
    /// Inherits properties like Id, Name, Description, IsSystem, and audit fields from <see cref="RoleDetailResponse"/>.
    /// </summary>
    public record Response : RoleDetailResponse;
}