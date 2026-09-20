namespace Shared.Security.Cors.Options;

public static class CorsResult
{
    public static class Failure
    {
        public static Error OriginsNull =>
            Error.Validation(
                "Cors.Origins.Null",
                "Origins must not be null.");

        public static Error AmbiguousOrigin =>
            Error.Validation(
                "Cors.Origins.AmbiguousOrigin",
                "Wildcard '*' cannot be combined with explicit origins.");
    }
}