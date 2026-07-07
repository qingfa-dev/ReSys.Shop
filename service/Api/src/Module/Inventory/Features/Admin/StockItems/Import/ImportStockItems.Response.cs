namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public record Response
    {
        public int Created { get; init; }
        public int Updated { get; init; }
        public int Failed { get; init; }
        public List<string> Errors { get; init; } = [];
    }
}
