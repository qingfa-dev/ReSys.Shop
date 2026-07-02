using FluentValidation;

namespace Shared.Performance.Caching.Options.Distributed;

/// <summary>
/// Validates <see cref="DistributedCacheSetting"/>.
/// </summary>
public sealed class DistributedCacheSettingValidator : AbstractValidator<DistributedCacheSetting>
{
    public DistributedCacheSettingValidator()
    {
        // Validate: Distributed cache type must be set and valid
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithErrorCode(DistributedCacheResult.Failure.TypeRequired.Code)
            .WithMessage(DistributedCacheResult.Failure.TypeRequired.Message)
            .Must(BeValidCacheType)
            .WithErrorCode(DistributedCacheResult.Failure.TypeInvalid.Code)
            .WithMessage(DistributedCacheResult.Failure.TypeInvalid.Message);

        // Validate: Distributed cache expiration must be greater than zero
        RuleFor(x => x.DefaultExpirationMinutes)
            .GreaterThanOrEqualTo(DistributedCacheConstant.Constraints.DefaultExpirationMinutesMin)
            .WithErrorCode(DistributedCacheResult.Failure.DefaultExpirationMinutesGreaterThanZero.Code)
            .WithMessage(DistributedCacheResult.Failure.DefaultExpirationMinutesGreaterThanZero.Message);
    }

    // Check: Returns true when type matches a known cache provider
    private static bool BeValidCacheType(string type)
    {
        return DistributedCacheConstant.Patterns.ValidTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
}
