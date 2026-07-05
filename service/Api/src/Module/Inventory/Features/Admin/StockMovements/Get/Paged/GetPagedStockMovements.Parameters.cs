namespace Module.Inventory.Features.Admin.StockMovements.Get.Paged;

public static partial class GetPagedStockMovements
{
    public record Parameters : QueryingParameters
    {
        public DateTimeOffset? FromUtc { get; init; }
        public DateTimeOffset? ToUtc { get; init; }
        public Guid? VariantId { get; init; }
        public Guid? StockLocationId { get; init; }
    }
}
