using FluentValidation;

namespace Shared.Security.Cors.Options;

public sealed class CorsSettingValidator : AbstractValidator<CorsSetting>
{
    public CorsSettingValidator()
    {
        RuleFor(x => x.Origins)
            .NotNull()
            .WithErrorCode(CorsResult.Failure.OriginsNull.Code)
            .WithMessage(CorsResult.Failure.OriginsNull.Message);

        RuleFor(x => x.Origins)
            .Must(origins =>
                origins is null ||
                !origins.Contains("*") ||
                origins.Length == 1)
            .WithErrorCode(CorsResult.Failure.AmbiguousOrigin.Code)
            .WithMessage(CorsResult.Failure.AmbiguousOrigin.Message);
    }
}