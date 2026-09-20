using FluentValidation;

using Microsoft.AspNetCore.Http;

namespace Shared.Security.Authentication.Guest.Options;

public sealed class GuestSessionSettingValidator : AbstractValidator<GuestSessionSetting>
{
    public GuestSessionSettingValidator()
    {
        RuleFor(x => x.CookieName)
            .NotEmpty()
            .WithErrorCode(GuestSessionSettingResult.Failure.CookieNameRequired.Code)
            .WithMessage(GuestSessionSettingResult.Failure.CookieNameRequired.Message)
            .Length(GuestSessionSettingConstant.Constraints.CookieNameMinLength, GuestSessionSettingConstant.Constraints.CookieNameMaxLength)
            .WithErrorCode(GuestSessionSettingResult.Failure.CookieNameInvalid.Code)
            .WithMessage(GuestSessionSettingResult.Failure.CookieNameInvalid.Message);

        RuleFor(x => x.CookieSameSite)
            .Must(static value => Enum.TryParse<SameSiteMode>(value, ignoreCase: true, out _))
            .WithErrorCode(GuestSessionSettingResult.Failure.CookieSameSiteInvalid.Code)
            .WithMessage(GuestSessionSettingResult.Failure.CookieSameSiteInvalid.Message);

        RuleFor(x => x.CookieSecurePolicy)
            .Must(static value => Enum.TryParse<CookieSecurePolicy>(value, ignoreCase: true, out _))
            .WithErrorCode(GuestSessionSettingResult.Failure.CookieSecurePolicyInvalid.Code)
            .WithMessage(GuestSessionSettingResult.Failure.CookieSecurePolicyInvalid.Message);

        RuleFor(x => x.ExpirationInDays)
            .InclusiveBetween(
                GuestSessionSettingConstant.Constraints.ExpirationInDaysMin,
                GuestSessionSettingConstant.Constraints.ExpirationInDaysMax)
            .WithErrorCode(GuestSessionSettingResult.Failure.ExpirationInDaysInvalid.Code)
            .WithMessage(GuestSessionSettingResult.Failure.ExpirationInDaysInvalid.Message);
    }
}
