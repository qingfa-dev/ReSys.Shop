using Module.Identity.Features.Shared.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Roles.Delete;

public static partial class DeleteRole
{
    public record Request : RoleRequest
    {
        public required Guid Id { get; init; }
    }
}