namespace Shared.Security.Authorization.Options;

public static class AuthzSettingResult
{
    public static class Failure
    {
        public static Error PermissionCacheSlidingExpirationPositive => Error.Validation(
            code: "Authorization.PermissionCache.SlidingExpiration.Positive",
            message:
            $"Permission cache sliding expiration must be at least {AuthzSettingConstant.Constraints.PermissionCacheSlidingExpirationMin} minutes.");

        public static Error PermissionCacheAbsoluteExpirationPositive => Error.Validation(
            code: "Authorization.PermissionCache.AbsoluteExpiration.Positive",
            message:
            $"Permission cache absolute expiration must be at least {AuthzSettingConstant.Constraints.PermissionCacheAbsoluteExpirationMin} minutes.");
    }
}