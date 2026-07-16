namespace Module.Inventory.Features.Storefront.StockAvailability.Check;

public static partial class GetStockAvailability
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public string? CartToken { get; init; }
    }
}
