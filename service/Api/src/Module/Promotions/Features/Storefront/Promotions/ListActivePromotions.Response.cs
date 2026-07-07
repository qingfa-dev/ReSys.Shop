namespace Module.Promotions.Features.Storefront.Promotions;

public static partial class ListActivePromotions
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public string Kind { get; init; } = null!;
        public string? Path { get; init; }
        public DateTimeOffset? ExpiresAtUtc { get; init; }
    }
}
