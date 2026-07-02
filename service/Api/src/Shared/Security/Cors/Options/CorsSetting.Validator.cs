using FluentValidation;

namespace Shared.Security.Cors.Options;

public sealed class CorsSettingValidator : AbstractValidator<CorsSetting>
{
    public CorsSettingValidator()
    {
        RuleFor(x => x.Origins)
            .NotNull()
            .WithErrorCode(CorsResult.Errors.OriginsNull.Code)
            .WithMessage(CorsResult.Errors.OriginsNull.Message);

        RuleFor(x => x.Origins)
            .Must(origins =>
                origins is null ||
                !origins.Contains("*") ||
                origins.Length == 1)
            .WithErrorCode(CorsResult.Errors.AmbiguousOrigin.Code)
            .WithMessage(CorsResult.Errors.AmbiguousOrigin.Message);
    }
}