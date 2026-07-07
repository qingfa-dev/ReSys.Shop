namespace Module.Ordering.Domain.LineItems;

// Initialize: Default constraints, limits, and query configuration for LineItem entity
public static class LineItemConstant
{
    public const int MaxQuantity = 999;

    public const byte Precision = 18;
    public const byte Scale = 2;

    public static class Defaults
    {
        public const int Quantity = 1;
    }

    public static class Query
    {
        public static readonly string[] AllowedSortFields =
        [
            nameof(LineItem.Quantity),
            nameof(LineItem.Price),
            nameof(LineItem.Total),
            nameof(LineItem.CreatedAtUtc)
        ];

        public static readonly string[] AllowedFilterFields =
        [
            nameof(LineItem.OrderId),
            nameof(LineItem.VariantId)
        ];
    }
}
