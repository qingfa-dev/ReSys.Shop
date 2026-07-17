using Shared.Security.Identity.Domain.Roles;

namespace Module.Identity.Domain.Roles;

public static class RoleConstant
{
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(Role.Name),
            nameof(Role.Description)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(Role.Name),
            nameof(Role.IsSystem),
            nameof(Role.CreatedAtUtc),
            nameof(Role.ModifiedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(Role.IsSystem),
            nameof(Role.CreatedAtUtc),
            nameof(Role.ModifiedAtUtc)
        ];
    }
}
