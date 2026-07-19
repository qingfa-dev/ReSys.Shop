namespace Module.Inventory.Features.Admin.StockLocations.Shared.Models;

public record StockLocationDetailResponse : StockLocationParameters, IResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}

public record StockLocationListItemResponse : StockLocationParameters, IResponse
{
    public Guid Id { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? ModifiedAtUtc { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
}