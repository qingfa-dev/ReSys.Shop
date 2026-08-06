namespace Module.Inventory.Features.Admin.StockMovements.Shared.Models;

public record StockMovementDetailResponse : StockMovementParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public record StockMovementListItemResponse : StockMovementParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}