namespace ReSys.ServiceDefaults.Constants;

public static class Images
{
    public static class Pgvector
    {
        public const string Optimized = "pgvector/pgvector:pg17-trixie";
        public const string Stable = "pgvector/pgvector:pg16-trixie";
    }

    public static class Redis
    {
        public const string Optimized = "redis:7-alpine";
        public const string Stable = "redis:7-bookworm";
    }

    public static class Papercut
    {
        public const string Optimized = "changemakerstudiosus/papercut-smtp";
        public const string Stable = "changemakerstudiosus/papercut-smtp";
    }
}
