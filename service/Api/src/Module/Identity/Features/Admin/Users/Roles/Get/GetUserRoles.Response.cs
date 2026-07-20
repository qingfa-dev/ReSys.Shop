using Module.Identity.Features.Admin.Roles.Shared.Models;

namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    public sealed record Response
    {
        public List<RoleItemResponse> Roles { get; init; } = default!;
    }

    public sealed record RoleItemResponse : RoleListResponse
    {
        public bool IsAssigned { get; init; }
    }
}
