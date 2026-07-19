namespace Module.Inventory.Features.Admin.StockMovements.Shared.Models;

public record StockMovementDetailResponse : StockMovementParameters, IResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public record StockMovementListItemResponse : StockMovementParameters, IResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}