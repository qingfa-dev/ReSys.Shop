namespace Module.Identity.Features.Admin.Users.Roles.Get;

public static partial class GetUserRoles
{
    // EXCEPTION: collection wrapper — inner RoleItemResponse is the domain DTO
    public sealed record Response(List<RoleItemResponse> Roles);

    public sealed record RoleItemResponse
    {
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public bool IsAssigned { get; init; }
    }
}
