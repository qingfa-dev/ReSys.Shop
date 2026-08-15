namespace Module.Inventory.Features.Admin.StockItems.Shared.Models;

public record StockItemRequest : StockItemParameters;

public abstract record StockItemImportParameters
{
    public required IFormFile File { get; init; }
}

public abstract record StockItemRestockParameters
{
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Reason { get; set; }
}

public abstract record StockItemLowStockParameters
{
    public Guid? LocationId { get; init; }
    public int? Threshold { get; init; }
}