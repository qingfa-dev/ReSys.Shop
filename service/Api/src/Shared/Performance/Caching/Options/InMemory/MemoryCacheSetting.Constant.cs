namespace Shared.Performance.Caching.Options.InMemory;

public static class MemoryCacheConstants
{
    public static class Constraints
    {
        public const int DefaultExpirationMinutesMin = 1;
        public const int DefaultExpirationMinutesMax = 1440; // 1 day

        public const int CompactionPercentageMin = 1;
        public const int CompactionPercentageMax = 100;

        // Optional: add size limits if needed
        public const long SizeLimitBytesMin = 1;
        public const long SizeLimitBytesMax = long.MaxValue;
    }

    public static class Defaults
    {
        public const bool Enabled = true;
        public const int DefaultExpirationMinutes = 30;
        public const int CompactionPercentage = 25;
        // If you add a size limit:
        public const long SizeLimitBytes = 100 * 1024 * 1024;
    }
}