namespace Shared.Performance.Caching.Options.Hybrid;

public static class HybridCacheSettingConstant
{
    public static class Constraints
    {
        public const int DefaultExpirationMinutesMin = 1;
        public const int DefaultExpirationMinutesMax = 1440; // optional: 1 day

        public const long MaximumPayloadBytesMin = 1;
        public const long MaximumPayloadBytesMax = 10 * 1024 * 1024; // 10 MB, for example

        public const int MaximumKeyLengthMin = 1;
        public const int MaximumKeyLengthMax = 2048;
    }

    public static class Defaults
    {
        public const bool Enabled = true;
        public const int DefaultExpirationMinutes = 30;
        public const long MaximumPayloadBytes = 1024 * 1024; // 1 MB
        public const int MaximumKeyLength = 1024;
    }
}