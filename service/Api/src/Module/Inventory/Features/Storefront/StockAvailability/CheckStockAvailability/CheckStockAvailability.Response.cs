namespace Module.Inventory.Features.Storefront.StockAvailability.CheckStockAvailability;

public static partial class CheckStockAvailability
{
    public sealed record Response
    {
        public Guid VariantId { get; init; }
        public bool IsAvailable { get; init; }
    }
}
