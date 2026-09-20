namespace Shared.Performance.Caching.Wrappers;

public partial class CacheService
{
    /// <summary>Structured log event definitions for cache hit/miss/set/remove lifecycle.</summary>
    internal static partial class Loggers
    {
        [LoggerMessage(
            EventId = 194,
            Level = LogLevel.Debug,
            Message = "Cache hit for key: {Key}")]
        public static partial void CacheHit(ILogger logger, string key);

        [LoggerMessage(
            EventId = 195,
            Level = LogLevel.Debug,
            Message = "Cache miss for key: {Key}")]
        public static partial void CacheMiss(ILogger logger, string key);

        [LoggerMessage(
            EventId = 196,
            Level = LogLevel.Information,
            Message = "Cache set for key: {Key}")]
        public static partial void CacheSet(ILogger logger, string key);

        [LoggerMessage(
            EventId = 197,
            Level = LogLevel.Information,
            Message = "Cache removed for key: {Key}")]
        public static partial void CacheRemoved(ILogger logger, string key);

        [LoggerMessage(
            EventId = 198,
            Level = LogLevel.Information,
            Message = "Cache removed by tag: {Tag}")]
        public static partial void CacheRemovedByTag(ILogger logger, string tag);

        [LoggerMessage(
            EventId = 199,
            Level = LogLevel.Warning,
            Message = "Cache bypassed for key: {Key} - caching is disabled")]
        public static partial void CacheBypassed(ILogger logger, string key);
    }
}
