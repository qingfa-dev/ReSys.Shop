namespace Module.Ordering.Features.Shared;

public static partial class OrderingFeature
{
    // @CAT-10 Boundary: Module → API — OpenAPI tag group constants consumed by Carter endpoint metadata
    public static class Tags
    {
        public static readonly string[] Order = ["Order"];
        public static readonly string[] Cart = ["Cart"];
    }
}