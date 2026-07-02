using FluentValidation;

namespace Shared.Performance.Caching.Options.Hybrid;

public sealed class HybridCacheSettingValidator : AbstractValidator<HybridCacheSetting>
{
    public HybridCacheSettingValidator()
    {
        RuleFor(x => x.DefaultExpirationMinutes)
            .InclusiveBetween(HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMin, HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMax)
            .WithErrorCode(DistributedCacheSettingResult.Failure.DefaultExpirationOutOfRange.Code)
            .WithMessage(DistributedCacheSettingResult.Failure.DefaultExpirationOutOfRange.Message);

        RuleFor(x => x.MaximumPayloadBytes)
            .InclusiveBetween(HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMin, HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMax)
            .WithErrorCode(DistributedCacheSettingResult.Failure.PayloadBytesOutOfRange.Code)
            .WithMessage(DistributedCacheSettingResult.Failure.PayloadBytesOutOfRange.Message);

        RuleFor(x => x.MaximumKeyLength)
            .InclusiveBetween(HybridCacheSettingConstant.Constraints.MaximumKeyLengthMin, HybridCacheSettingConstant.Constraints.MaximumKeyLengthMax)
            .WithErrorCode(DistributedCacheSettingResult.Failure.KeyLengthOutOfRange.Code)
            .WithMessage(DistributedCacheSettingResult.Failure.KeyLengthOutOfRange.Message);
    }
}