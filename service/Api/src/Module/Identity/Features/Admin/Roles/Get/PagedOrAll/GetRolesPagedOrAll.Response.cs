using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Get.PagedOrAll;

public static partial class GetRolesPagedOrAll
{
    /// <summary>
    /// Represents the response contract for a list of roles, typically used in paged results.
    /// Inherits properties like Id, Name, Description, and IsSystem from <see cref="RoleListResponse"/>.
    /// </summary>
    public class Response : RoleListResponse { }
}