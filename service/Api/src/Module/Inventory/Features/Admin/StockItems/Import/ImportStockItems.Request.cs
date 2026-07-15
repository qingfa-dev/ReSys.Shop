namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public sealed record Request
    {
        public required IFormFile File { get; init; }
    }
}
