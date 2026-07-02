using Shared.Governance.Conventions;

namespace Shared.Security.Identity.Domain.Shared;

public static class IdentitySchema
{
    public static string Name => "Identity".ToSnakeCase()!;

    public static class TableNames
    {
        public static string Users => nameof(Users).ToSnakeCase()!;
        public static string Roles => nameof(Roles).ToSnakeCase()!;
        public static string UserRoles => nameof(UserRoles).ToSnakeCase()!;
        public static string UserClaims => nameof(UserClaims).ToSnakeCase()!;
        public static string UserLogins => nameof(UserLogins).ToSnakeCase()!;
        public static string UserTokens => nameof(UserTokens).ToSnakeCase()!;
        public static string RoleClaims => nameof(RoleClaims).ToSnakeCase()!;
        public static string Addresses => "Addresses".ToSnakeCase()!;
        public static string RefreshTokens => nameof(RefreshTokens).ToSnakeCase()!;
        public static string Passkeys => nameof(Passkeys).ToSnakeCase()!;
    }
}
