namespace Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

public static partial class CheckStockAvailability
{
    public sealed record Request
    {
        public Guid VariantId { get; init; }
        public int Quantity { get; init; }
    }
}
