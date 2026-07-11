using FluentValidation;

using Microsoft.AspNetCore.Http;

namespace Shared.Security.AntiForgery.Options;

public sealed class AntiForgerySettingValidator : AbstractValidator<AntiForgerySetting>
{
    public AntiForgerySettingValidator()
    {
        RuleFor(x => x.HeaderName)
            .NotEmpty()
            .WithErrorCode(AntiForgerySettingResult.Failure.HeaderNameRequired.Code)
            .WithMessage(AntiForgerySettingResult.Failure.HeaderNameRequired.Message)
            .Length(AntiForgerySettingConstant.Constraints.HeaderNameMinLength, AntiForgerySettingConstant.Constraints.HeaderNameMaxLength)
            .WithErrorCode(AntiForgerySettingResult.Failure.HeaderNameInvalid.Code)
            .WithMessage(AntiForgerySettingResult.Failure.HeaderNameInvalid.Message);

        RuleFor(x => x.CookieName)
            .NotEmpty()
            .WithErrorCode(AntiForgerySettingResult.Failure.CookieNameRequired.Code)
            .WithMessage(AntiForgerySettingResult.Failure.CookieNameRequired.Message)
            .Length(AntiForgerySettingConstant.Constraints.CookieNameMinLength, AntiForgerySettingConstant.Constraints.CookieNameMaxLength)
            .WithErrorCode(AntiForgerySettingResult.Failure.CookieNameInvalid.Code)
            .WithMessage(AntiForgerySettingResult.Failure.CookieNameInvalid.Message);

        RuleFor(x => x.CookieSameSite)
            .Must(static value => Enum.TryParse<SameSiteMode>(value, ignoreCase: true, out _))
            .WithErrorCode(AntiForgerySettingResult.Failure.CookieSameSiteInvalid.Code)
            .WithMessage(AntiForgerySettingResult.Failure.CookieSameSiteInvalid.Message);

        RuleFor(x => x.CookieSecurePolicy)
            .Must(static value => Enum.TryParse<CookieSecurePolicy>(value, ignoreCase: true, out _))
            .WithErrorCode(AntiForgerySettingResult.Failure.CookieSecurePolicyInvalid.Code)
            .WithMessage(AntiForgerySettingResult.Failure.CookieSecurePolicyInvalid.Message);

        When(x => x.CookieMaxAgeMinutes.HasValue, (Action)(() =>
        {
            RuleFor(x => x.CookieMaxAgeMinutes!.Value)
                .GreaterThan(0)
                .WithErrorCode(AntiForgerySettingResult.Failure.CookieMaxAgeMinutesInvalid.Code)
                .WithMessage((string)AntiForgerySettingResult.Failure.CookieMaxAgeMinutesInvalid.Message);
        }));
    }
}
