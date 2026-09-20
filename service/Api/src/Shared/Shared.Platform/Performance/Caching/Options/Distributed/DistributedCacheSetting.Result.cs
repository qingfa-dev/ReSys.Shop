namespace Shared.Performance.Caching.Options.Distributed;

public static class DistributedCacheResult
{
    public static class Failure
    {
        public static Error TypeRequired => Error.Validation(
            code: "Caching.Distributed.Type.Required",
            message: "Distributed cache type is required.");

        public static Error TypeInvalid => Error.Validation(
            code: "Caching.Distributed.Type.Invalid",
            message: $"Distributed cache type must be '{string.Join("', '", DistributedCacheConstant.Patterns.ValidTypes)}'.");

        public static Error DefaultExpirationMinutesGreaterThanZero => Error.Validation(
            code: "Caching.Distributed.DefaultExpirationMinutes.GreaterThanZero",
            message: $"Distributed cache default expiration must be greater than {DistributedCacheConstant.Constraints.DefaultExpirationMinutesMin} minutes.");
    }
}
