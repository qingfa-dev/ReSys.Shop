using Module.Identity.Features.Shared.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Update;

public static partial class UpdateRole
{
    /// <summary>
    /// Represents the response contract for an updated role.
    /// Inherits properties like Id, Name, Description, IsSystem, and audit fields from <see cref="RoleDetailResponse"/>.
    /// </summary>
    public record Response : RoleDetailResponse;
}