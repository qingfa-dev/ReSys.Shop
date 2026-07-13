namespace Module.Inventory.Features.Admin.StockMovements.Shared.Models;

public class StockMovementDetailResponse : StockMovementParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}

public class StockMovementListItemResponse : StockMovementParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}