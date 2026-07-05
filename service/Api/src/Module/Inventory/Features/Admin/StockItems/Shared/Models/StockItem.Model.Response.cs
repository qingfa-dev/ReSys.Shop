namespace Module.Inventory.Features.Admin.StockItems.Shared.Models;

public class StockItemDetailResponse : StockItemParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

public class StockItemListItemResponse : StockItemParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
