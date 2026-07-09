using FluentValidation;

namespace Shared.Security.RateLimiting.Options;

public sealed class RateLimitSettingValidator : AbstractValidator<RateLimitSetting>
{
    public RateLimitSettingValidator()
    {
        RuleFor(x => x.Policies)
            .Must(policies => policies.Values.All(p => p.PermitLimit > 0))
            .WithErrorCode(RateLimitResult.Failure.PermitLimitZero.Code)
            .WithMessage(RateLimitResult.Failure.PermitLimitZero.Message);

        RuleFor(x => x.Policies)
            .Must(policies => policies.Values.All(p => p.WindowSeconds > 0))
            .WithErrorCode(RateLimitResult.Failure.WindowSecondsZero.Code)
            .WithMessage(RateLimitResult.Failure.WindowSecondsZero.Message);
    }
}
