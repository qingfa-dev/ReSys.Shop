using Shared.Security.Identity.Domain.Users;

namespace Module.Identity.Domain.Users;

public static class UserConstant
{
    public static class Query
    {
        public static readonly string[] AllowedSearchFields =
        [
            nameof(User.UserName),
            nameof(User.Email),
            nameof(User.FirstName),
            nameof(User.LastName)
        ];

        public static readonly string[] AllowedSortFields =
        [
            nameof(User.UserName),
            nameof(User.Email),
            nameof(User.CreatedAtUtc),
            nameof(User.ModifiedAtUtc),
            nameof(User.LastLoginAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(User.IsActive),
            nameof(User.EmailConfirmed),
            nameof(User.PhoneNumberConfirmed),
            nameof(User.CreatedAtUtc),
            nameof(User.ModifiedAtUtc)
        ];
    }
}
