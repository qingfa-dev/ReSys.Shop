using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Roles.Delete;

public static partial class DeleteRole
{
    public class Request : RoleRequest
    {
        public required Guid Id { get; init; }
    }
}
