namespace Shared.Performance.Caching.Options.Hybrid;

public static class DistributedCacheSettingResult
{
    public static class Failure
    {
        public static Error DefaultExpirationOutOfRange => Error.Validation(
            "Caching.Hybrid.DefaultExpiration.OutOfRange",
            $"Default expiration must be between {HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMin} and {HybridCacheSettingConstant.Constraints.DefaultExpirationMinutesMax} minutes.");

        public static Error PayloadBytesOutOfRange => Error.Validation(
            "Caching.Hybrid.MaximumPayloadBytes.OutOfRange",
            $"Maximum payload size must be between {HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMin} and {HybridCacheSettingConstant.Constraints.MaximumPayloadBytesMax} bytes.");

        public static Error KeyLengthOutOfRange => Error.Validation(
            "Caching.Hybrid.MaximumKeyLength.OutOfRange",
            $"Maximum key length must be between {HybridCacheSettingConstant.Constraints.MaximumKeyLengthMin} and {HybridCacheSettingConstant.Constraints.MaximumKeyLengthMax} characters.");
    }
}
