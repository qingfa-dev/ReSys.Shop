namespace Shared.Performance.Caching.Options.Distributed;

public static class DistributedCacheConstant
{
    public static class Constraints
    {
        public const int DefaultExpirationMinutesMin = 1;
    }

    public static class Patterns
    {
        public static readonly string[] ValidTypes = ["redis", "sqlserver"];
    }
}
