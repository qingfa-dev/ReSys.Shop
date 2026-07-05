namespace Module.Inventory.Features.Admin.StockLocations.Shared.Models;

public class StockLocationDetailResponse : StockLocationParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}

public class StockLocationListItemResponse : StockLocationParameters
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ModifiedAtUtc { get; set; }
}
