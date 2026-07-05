namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public sealed class Response
    {
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public List<ImportError> Errors { get; set; } = [];
    }

    public sealed class ImportError
    {
        public int Row { get; set; }
        public string? VariantId { get; set; }
        public string Error { get; set; } = string.Empty;
    }
}
