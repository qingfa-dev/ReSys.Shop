namespace Module.Inventory.Features.Admin.StockItems.Shared.Models;

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