using Module.Identity.Features.Admin.Shared.Models;

namespace Module.Identity.Features.Shared.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    public sealed record Response : RoleListResponse
    {
        public bool IsAssigned { get; init; }
    }
}
