namespace Module.Inventory.Features.Admin.StockMovements.Shared.Models;

public abstract class StockMovementParameters
{
    public Guid StockItemId { get; init; }
    public int Quantity { get; init; }
    public int PreviousCountOnHand { get; init; }
    public string? Action { get; init; }
    public string? Reason { get; init; }
    public string? OriginatorType { get; init; }
    public Guid? OriginatorId { get; init; }
}