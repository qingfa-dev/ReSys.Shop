using FluentValidation;

namespace Shared.Security.Authorization.Options;

public sealed partial class AuthzSettingValidator : AbstractValidator<AuthzSetting>
{

    public AuthzSettingValidator()
    {
        RuleFor(x => x.PermissionCache).NotNull();

        RuleFor(x => x.PermissionCache.SlidingExpiration)
            .GreaterThanOrEqualTo(TimeSpan.FromMinutes(AuthzSettingConstant.Constraints.PermissionCacheSlidingExpirationMin))
            .WithErrorCode(AuthzSettingResult.Failure.PermissionCacheSlidingExpirationPositive.Code)
            .WithMessage(AuthzSettingResult.Failure.PermissionCacheSlidingExpirationPositive.Message)
            .When(x => x.PermissionCache != null);

        RuleFor(x => x.PermissionCache.AbsoluteExpiration)
            .GreaterThanOrEqualTo(TimeSpan.FromMinutes(AuthzSettingConstant.Constraints.PermissionCacheAbsoluteExpirationMin))
            .WithErrorCode(AuthzSettingResult.Failure.PermissionCacheAbsoluteExpirationPositive.Code)
            .WithMessage(AuthzSettingResult.Failure.PermissionCacheAbsoluteExpirationPositive.Message)
            .When(x => x.PermissionCache != null);
    }
}
