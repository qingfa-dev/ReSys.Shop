namespace Shared.Performance.Caching.Options;

public static class CachingSettingResult
{
    public static class Failure
    {
        // Sub‑options required
        public static Error MemoryRequired =>
            Error.Validation(
                code: "Caching.Memory.Required",
                message: "Memory cache options section is required.");

        public static Error DistributedRequired =>
            Error.Validation(
                code: "Caching.Distributed.Required",
                message: "Distributed cache options section is required.");

        public static Error HybridRequired =>
            Error.Validation(
                code: "Caching.Hybrid.Required",
                message: "Hybrid cache options section is required.");

        // Connection string missing (dynamic)
        public static Error ConnectionStringMissing(string connectionStringName) =>
            Error.Validation(
                code: "Caching.ConnectionString.Missing",
                message: $"Connection string '{connectionStringName}' is required when distributed caching is enabled. " +
                             $"Please configure it in appsettings.json or via environment variable 'ConnectionStrings__{connectionStringName}'.");
    }
}