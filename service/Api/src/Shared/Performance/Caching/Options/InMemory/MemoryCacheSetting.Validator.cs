using FluentValidation;

namespace Shared.Performance.Caching.Options.InMemory;

public sealed class MemoryCacheSettingValidator : AbstractValidator<MemoryCacheSetting>
{
    public MemoryCacheSettingValidator()
    {
        RuleFor(x => x.DefaultExpirationMinutes)
            .InclusiveBetween(MemoryCacheConstants.Constraints.DefaultExpirationMinutesMin, MemoryCacheConstants.Constraints.DefaultExpirationMinutesMax)
            .WithErrorCode(MemoryCacheResult.Failure.DefaultExpirationOutOfRange.Code)
            .WithMessage(MemoryCacheResult.Failure.DefaultExpirationOutOfRange.Message);

        RuleFor(x => x.CompactionPercentage)
            .InclusiveBetween(MemoryCacheConstants.Constraints.CompactionPercentageMin, MemoryCacheConstants.Constraints.CompactionPercentageMax)
            .WithErrorCode(MemoryCacheResult.Failure.CompactionPercentageOutOfRange.Code)
            .WithMessage(MemoryCacheResult.Failure.CompactionPercentageOutOfRange.Message);

        // If you add a size limit:
        When(x => x.SizeLimitBytes.HasValue, () =>
        {
            RuleFor(x => x.SizeLimitBytes!.Value)
                .GreaterThanOrEqualTo(MemoryCacheConstants.Constraints.SizeLimitBytesMin)
                .WithErrorCode(MemoryCacheResult.Failure.SizeLimitOutOfRange.Code)
                .WithMessage(MemoryCacheResult.Failure.SizeLimitOutOfRange.Message);
        });
    }
}