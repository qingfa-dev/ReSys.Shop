namespace Module.Inventory.Features.Admin.Shared.Models;

public abstract record class StockItemParameters
{
    public Guid StockLocationId { get; init; }
    public Guid VariantId { get; init; }
    public int CountOnHand { get; init; }
    public bool Backorderable { get; init; }
}

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

public record ImportStockItemsResponseBase
{
    public int Created { get; init; }
    public int Updated { get; init; }
    public int Failed { get; init; }
    public List<string> Errors { get; init; } = [];
}

public record StockItemDetailResponse : StockItemParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record StockItemListItemResponse : StockItemParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record RestockResultResponse
{
    public Guid StockItemId { get; init; }
    public int PreviousCountOnHand { get; init; }
    public int NewCountOnHand { get; init; }
    public int BackordersFulfilled { get; init; }
    public int PartiallyFulfilled { get; init; }
    public int RemainingQuantity { get; init; }
    public Guid? MovementId { get; init; }
}

public record StockSummaryDetailResponse
{
    public Guid VariantId { get; init; }
    public int TotalOnHand { get; init; }
    public int TotalReserved { get; init; }
    public int TotalAvailable { get; init; }
    public List<LocationBreakdownItem> LocationBreakdown { get; init; } = [];
}

public record LocationBreakdownItem
{
    public Guid LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public int CountOnHand { get; init; }
    public int Reserved { get; init; }
    public int Available { get; init; }
    public bool IsLowStock { get; init; }
}
