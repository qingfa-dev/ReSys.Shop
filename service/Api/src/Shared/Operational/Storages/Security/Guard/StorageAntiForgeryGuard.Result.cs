namespace Shared.Operational.Storages.Security.Guard;

public static class StorageAntiForgeryGuardResult
{
    public static class Failure
    {
        public static Error TooManyAttempts()
            => Error.Conflict("Storage.TooManyAttempts", "Upload blocked due to too many recent failures.");
    }
}
