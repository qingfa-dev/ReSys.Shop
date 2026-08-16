namespace Module.Inventory.Features.Admin.Shared.Models;

public abstract record StockMovementParameters
{
    public Guid StockItemId { get; init; }
    public int Quantity { get; init; }
    public int PreviousCountOnHand { get; init; }
    public string? Action { get; init; }
    public string? Reason { get; init; }
    public string? OriginatorType { get; init; }
    public Guid? OriginatorId { get; init; }
}

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
