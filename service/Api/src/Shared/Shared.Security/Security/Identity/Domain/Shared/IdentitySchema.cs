public static class IdentitySchema
{
    public static string Name => "identity";

    public static class TableNames
    {
        public static string Users => "users";
        public static string Roles => "roles";
        public static string UserRoles => "user_roles";
        public static string UserClaims => "user_claims";
        public static string UserLogins => "user_logins";
        public static string UserTokens => "user_tokens";
        public static string RoleClaims => "role_claims";
        public static string Addresses => "addresses";
        public static string RefreshTokens => "refresh_tokens";
        public static string Passkeys => "passkeys";
    }
}
