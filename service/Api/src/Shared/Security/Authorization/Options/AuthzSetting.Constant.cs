namespace Shared.Security.Authorization.Options;

public static class AuthzSettingConstant
{
    public static class Constraints
    {
        public const double PermissionCacheSlidingExpirationMin = 0;
        public const double PermissionCacheAbsoluteExpirationMin = 0;
    }

    public static class Defaults
    {
        public const double PermissionCacheSlidingExpirationMinutes = 5;
        public const double PermissionCacheAbsoluteExpirationMinutes = 30;
    }
}
