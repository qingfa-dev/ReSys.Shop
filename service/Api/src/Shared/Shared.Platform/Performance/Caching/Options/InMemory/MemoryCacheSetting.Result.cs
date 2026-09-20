namespace Shared.Performance.Caching.Options.InMemory;

public static class MemoryCacheResult
{
    public static class Failure
    {
        public static Error DefaultExpirationOutOfRange =>
            Error.Validation(
            "Caching.Memory.DefaultExpiration.OutOfRange",
            $"Memory cache default expiration must be between {MemoryCacheConstants.Constraints.DefaultExpirationMinutesMin} and {MemoryCacheConstants.Constraints.DefaultExpirationMinutesMax} minutes.");

        public static Error CompactionPercentageOutOfRange =>
            Error.Validation(
                "Caching.Memory.CompactionPercentage.OutOfRange",
                $"Memory cache compaction percentage must be between {MemoryCacheConstants.Constraints.CompactionPercentageMin} and {MemoryCacheConstants.Constraints.CompactionPercentageMax}.");

        // If size limit is used:
        public static Error SizeLimitOutOfRange =>
            Error.Validation(
                "Caching.Memory.SizeLimit.OutOfRange",
                $"Memory cache size limit must be at least {MemoryCacheConstants.Constraints.SizeLimitBytesMin} byte(s).");
    }
}